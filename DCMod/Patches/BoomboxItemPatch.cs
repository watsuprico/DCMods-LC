using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace DCMod.Patches {
    public class BoomboxItemPatch {
        [HarmonyPatch(typeof(BoomboxItem), nameof(BoomboxItem.Start))]
        [HarmonyPostfix]
        public static void Start(BoomboxItem __instance) {
            if (AudioVideoManager.BoomboxAudioClips.Count > 0)
                AudioVideoManager.ApplyClipsToBoombox(__instance);
            else
                AudioVideoManager.OnAllBoomboxAudioClipsLoaded += () => AudioVideoManager.ApplyClipsToBoombox(__instance);

            try {
                bool hasAddedThisBoombox = false;
                foreach (var volumeController in AudioVideoManager.VolumeControllers) {
                    if (volumeController == null || volumeController.GetType() != typeof(VolumeControllerGrabbable))
                        continue;
                    if (((VolumeControllerGrabbable)volumeController).Grabbable == __instance.grabbable) {
                        hasAddedThisBoombox = true;
                        break;
                    }
                }
                if (!hasAddedThisBoombox)
                    AudioVideoManager.VolumeControllers.Add(new VolumeControllerGrabbable(__instance));
            } catch (Exception e) {
                Plugin.Logger.LogWarning($"Cannot add boombox volume controller due to exception: {e.Message}");
                Plugin.Logger.LogWarning($"{e.StackTrace}");
            }
        }



        [HarmonyPatch(typeof(BoomboxItem), "StartMusic")]
        [HarmonyPostfix]
        public static void StartMusic(BoomboxItem __instance, bool startMusic) {
            Plugin.Logger.LogDebug($"{nameof(BoomboxItemPatch)}::{nameof(StartMusic)}(startMusic: {startMusic}): boomboxAudio.clip.name: {__instance?.boomboxAudio?.clip?.name}");
        }


        [HarmonyPatch(typeof(BoomboxItem), nameof(BoomboxItem.PocketItem))]
        [HarmonyPostfix]
        public static void PocketItem(BoomboxItem __instance) {
            if (PluginConfig.StopBoomboxIfPocketed) {
                var startMusicMethod = typeof(BoomboxItem).GetMethod("StartMusic", BindingFlags.NonPublic | BindingFlags.Instance);

                // Check if the method is found
                if (startMusicMethod != null) startMusicMethod.Invoke(__instance, new object[] { false, false });
                else {
                    Plugin.Logger.LogWarning($"{nameof(BoomboxItemPatch)}::{nameof(PocketItem)}() -> unable to find StartMusic method for BoomboxItem instance! Cannot stop the music.");
                }
            }
        }
    }


    /// <summary>
    /// Removes the BoomboxItem::StartMusic call (we'll do that)
    /// </summary>
    [HarmonyPatch(typeof(BoomboxItem), nameof(BoomboxItem.PocketItem))]
    public class BoomboxItemPocketItem {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            var opcodes = instructions.ToList();
            int removeCount = 4;
            int startMusicCallIndex = -1;
            for (int i = 0; i < opcodes.Count; i++) {
                var opcode = opcodes[i];
                if (opcode.opcode == OpCodes.Call
                    && opcode.operand.ToString().Contains("StartMusic", StringComparison.OrdinalIgnoreCase)) {
                    startMusicCallIndex = i;
                }
            }

            if (startMusicCallIndex != -1) opcodes.RemoveRange(startMusicCallIndex - (removeCount - 1), removeCount);

            return opcodes.AsEnumerable();
        }
    }
}
