using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine.InputSystem;

namespace DCMod.Patches {
    public class PlayerControllerBPatch {
        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPrefix]
        public static void Update(PlayerControllerB __instance) {
            if (!__instance.isPlayerControlled
                || __instance.inTerminalMenu
                || __instance.isTypingChat
                || __instance.isPlayerDead) {
                return;
            }

            // Find which VolumeController we're looking at.
            try {
                var lookingAtObject = __instance.hoveringOverTrigger?.transform.parent?.gameObject;

                foreach (var volumeController in AudioVideoManager.VolumeControllers) {
                    if (volumeController == null)
                        continue;

                    bool isFurnitureVC = volumeController.GetType() == typeof(VolumeControllerFurniture);
                    bool isGrabbableVC = volumeController.GetType() == typeof(VolumeControllerGrabbable);

                    if (lookingAtObject != null && (lookingAtObject.name.Contains("RecordPlayer", System.StringComparison.OrdinalIgnoreCase) || lookingAtObject.name.Contains("Television", System.StringComparison.OrdinalIgnoreCase))) {
                        if (isFurnitureVC && ((VolumeControllerFurniture)volumeController).Trigger == __instance.hoveringOverTrigger)
                            volumeController.BeingLookedAtByPlayer = true;
                        else
                            volumeController.BeingLookedAtByPlayer = false;
                    } else if ( // I'm so sorry for this...
                                  isGrabbableVC // VolumeController is for a Grabbable item
                                  && (
                                      (!string.IsNullOrEmpty(__instance.cursorTip.text) && __instance.cursorTip.text.Contains(volumeController.CurrentToolTip, System.StringComparison.OrdinalIgnoreCase)) // We're looking at a boombox (maybe?)
                                      || __instance.currentlyHeldObjectServer.GetType() == typeof(BoomboxItem) // OR we're holding a boombox
                                 )
                              ) {
                        volumeController.BeingLookedAtByPlayer = true;
                    } else {
                        volumeController.BeingLookedAtByPlayer = false;
                    }
                }
            } catch { }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        public static void PostUpdate(PlayerControllerB __instance) {

            // VolumeController
            if (!__instance.isPlayerControlled
                || __instance.inTerminalMenu
                || __instance.isTypingChat
                || __instance.isPlayerDead) {
                return;
            }

            bool updateAllAudioSources = Keyboard.current[PluginConfig.AdjustAllItemsVolumeKey].isPressed;

            if (Keyboard.current[PluginConfig.VolumeUpKey].wasPressedThisFrame)
                AudioVideoManager.VolumeUp(updateAllAudioSources);
            if (Keyboard.current[PluginConfig.VolumeDownKey].wasPressedThisFrame)
                AudioVideoManager.VolumeDown(updateAllAudioSources);
        }
    }
}
