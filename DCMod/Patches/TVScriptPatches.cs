using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine.Video;
using UnityEngine;

namespace DCMod.Patches {
    internal class TVScriptPatches {

        private static bool _havePlayedOnTV = false;
        private static RenderTexture? _renderTexture;

        private static readonly FieldInfo currentClipProperty = typeof(TVScript).GetField("currentClip", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo currentTimeProperty = typeof(TVScript).GetField("currentClipTime", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo SetTVScreenMaterialMethod = typeof(TVScript).GetMethod("SetTVScreenMaterial", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo OnEnableMethod = typeof(TVScript).GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic);

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TVScript), "Update")]
        public static bool Update(TVScript __instance) {
            // this is called .. a lot, a lot.
            //Plugin.Logger.LogDebug($"{nameof(TVScriptPatches)}::{nameof(Update)}()");

            // What's playing??
            if (AudioVideoManager.CurrentTVVideoPlayer == null) {
                AudioVideoManager.CurrentTVVideoPlayer = __instance.GetComponent<VideoPlayer>();
                _renderTexture = AudioVideoManager.CurrentTVVideoPlayer.targetTexture;
                // Not our stuff, play our stuff
                if (AudioVideoManager.VideoURIs.Count > 0) PrepareVideo(__instance, 0);
            }
            return false;
        }

        [HarmonyPatch(typeof(TVScript), nameof(TVScript.TurnTVOnOff))]
        [HarmonyPrefix]
        public static bool TurnTVOnOff(TVScript __instance, bool on) {
            string tag = $"{nameof(TVScriptPatches)}::{nameof(TurnTVOnOff)}(on: {on})";
            Plugin.Logger.LogDebug(tag);

            if (AudioVideoManager.VideoURIs.Count == 0)
                return false; // We have no videos, let the game figure it out.

            var num = (int)currentClipProperty.GetValue(__instance);
            if (on && _havePlayedOnTV) {
                num = (num + 1) % AudioVideoManager.VideoURIs.Count;
                currentClipProperty.SetValue(__instance, num);
            }

            if (on) {
                try {
                    // We do this because I'm simi-lazy and need to always re-add the TV audio on new rounds.
                    bool hasSetupVolumeController = false;
                    var interactTrigger = __instance.GetComponentInChildren<InteractTrigger>();
                    foreach (var volumeController in AudioVideoManager.VolumeControllers) {
                        if (volumeController == null || volumeController.GetType() != typeof(VolumeControllerFurniture))
                            continue;

                        if (((VolumeControllerFurniture)volumeController).Trigger == interactTrigger) {
                            hasSetupVolumeController = true;
                            break;
                        }
                    }

                    if (!hasSetupVolumeController)
                        AudioVideoManager.VolumeControllers.Add(new VolumeControllerFurniture(__instance));
                } catch (Exception e) {
                    Plugin.Logger.LogWarning($"{tag}: Unable to check if the record player has already been add to the volume controllers. (Exception thrown: {e.Message}).");
                    Plugin.Logger.LogDebug(e.StackTrace);
                }
            }

            __instance.tvOn = on;
            if (on) {
                PlayVideo(__instance);
                __instance.tvSFX.PlayOneShot(__instance.switchTVOn);
                WalkieTalkie.TransmitOneShotAudio(__instance.tvSFX, __instance.switchTVOn);
            } else {
                __instance.video.Stop();
                __instance.tvSFX.PlayOneShot(__instance.switchTVOff);
                WalkieTalkie.TransmitOneShotAudio(__instance.tvSFX, __instance.switchTVOff);
            }
            SetTVScreenMaterialMethod.Invoke(__instance, new object[1] { on });
            return false;
        }


        [HarmonyPatch(typeof(TVScript), "TVFinishedClip")]
        [HarmonyPrefix]
        public static bool TVFinishedClip(TVScript __instance, VideoPlayer source) {
            Plugin.Logger.LogDebug($"{nameof(TVScriptPatches)}::{nameof(TVFinishedClip)}(source: {source.url})");

            if (!__instance.tvOn || GameNetworkManager.Instance.localPlayerController.isInsideFactory)
                return false;


            int num = (int)currentClipProperty.GetValue(__instance);
            if (AudioVideoManager.VideoURIs.Count > 0) num = (num + 1) % AudioVideoManager.VideoURIs.Count;
            currentTimeProperty.SetValue(__instance, 0f);
            currentClipProperty.SetValue(__instance, num);
            PlayVideo(__instance);
            return false;
        }

        private static void PrepareVideo(TVScript instance, int index = -1) {
            if (index == -1) index = (int)currentClipProperty.GetValue(instance) + 1;
            AudioVideoManager.PrepareVideo(instance, index);
        }

        private static void PlayVideo(TVScript instance) {
            _havePlayedOnTV = true;
            if (AudioVideoManager.VideoURIs.Count != 0) {
                if (AudioVideoManager.NextTVVideoPlayer != null) {
                    var videoPlayer = AudioVideoManager.CurrentTVVideoPlayer;
                    instance.video = AudioVideoManager.CurrentTVVideoPlayer = AudioVideoManager.NextTVVideoPlayer;
                    AudioVideoManager.NextTVVideoPlayer = null;

                    if (videoPlayer != null) {
                        Plugin.Logger.LogDebug($"Destroyed {videoPlayer}");
                        UnityEngine.Object.Destroy(videoPlayer);
                    }

                    OnEnableMethod.Invoke(instance, new object[0]);
                }
                currentTimeProperty.SetValue(instance, 0f);
                if (_renderTexture != null)
                    instance.video.targetTexture = _renderTexture;
                instance.video.Play();
                PrepareVideo(instance);
            }
        }
    }
}
