using HarmonyLib;
using System;
using UnityEngine;

namespace DCMod.Patches {
    public class AnimatedObjectTriggerPatch {
        [HarmonyPatch(typeof(AnimatedObjectTrigger), "PlayAudio")]
        [HarmonyPrefix]
        public static void PlayAudio(ref AnimatedObjectTrigger __instance, ref AudioClip ___playWhileTrue, bool boolVal, bool playSecondaryAudios = false) {
            string tag = $"{nameof(AnimatedObjectTriggerPatch)}::{nameof(PlayAudio)}(boolVal: {boolVal}, playSecondaryAudios: {playSecondaryAudios})";
            try {
                Plugin.Logger.LogDebug(tag);

                bool isRecordPlayer = false;
                var componentsInParent = __instance.gameObject.GetComponentsInParent<Component>();
                foreach (var component in componentsInParent) {
                    if (component == null || component.name == null || !component.name.Contains("RecordPlayer")) // only touch the RecordPlayer, not the light switch or something
                        continue;
                    isRecordPlayer = true;
                    break;
                }

                if (!isRecordPlayer || !boolVal) // boolVal will be true for playing audio
                    return;

                AudioVideoManager.PlayARecordPlayerClip();
            } catch (Exception e) {
                Plugin.Logger.LogError($"{tag}: error occurred. Null reference?? Error: {e.Message}");
                Plugin.Logger.LogError($"---STACK TRACE--- {Environment.NewLine}{e.StackTrace}");
            }
        }
    }
}
