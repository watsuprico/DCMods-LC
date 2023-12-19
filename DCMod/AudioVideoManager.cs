using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

namespace DCMod {
    public class AudioVideoManager {

        // Properties
        #region Video
        /// <summary>
        /// TV video file paths.
        /// </summary>
        public static List<string> VideoURIs = [];
        /// <summary>
        /// Current video playing on the TV.
        /// </summary>
        public static VideoPlayer? CurrentTVVideoPlayer;
        /// <summary>
        /// Next video to be played on the TV.
        /// </summary>
        public static VideoPlayer? NextTVVideoPlayer;
        #endregion

        #region Audio
        /// <summary>
        /// Audio files to be played on all audio devices (boombox and record players).
        /// </summary>
        public static List<string> AudioURIs = [];
        private static List<string> BoomboxOnlyAudioURIs = [];
        private static List<string> RecordPlayerOnlyAudioURIs = [];

        /// <summary>
        /// Audio files for the boombox.
        /// </summary>
        public static List<string> BoomboxAudioURIs {
            get => [.. BoomboxOnlyAudioURIs, .. AudioURIs];
        }

        /// <summary>
        /// Audio files for the record category.
        /// </summary>
        public static List<string> RecordPlayerAudioURIs {
            get => [.. RecordPlayerOnlyAudioURIs, .. AudioURIs];
        }


        public static List<AudioClip> BoomboxAudioClips = [];
        public static List<AudioClip> RecordPlayerClips = [];
        private static bool copiedGameRecordPlayerClips = false;

        private static bool _loadingBoomboxAudioClips = false;
        public static event Action? OnAllBoomboxAudioClipsLoaded;
        private static bool _loadingRecordPlayerAudioClips = false;
        public static event Action? OnAllRecordPlayerAudioClipsLoaded;

        /// <summary>
        /// Volume controller we're managing.
        /// </summary>
        public static List<VolumeController> VolumeControllers = [];

        #endregion

        public AudioVideoManager() { }

        // Methods

        #region Video
        public static void LoadVideoURIs() {
            try {
                var directory = Path.Join(Plugin.PluginDirectory, "TV_Videos");

                string[] files = [];
                if (Directory.Exists(directory))
                    files = Directory.GetFiles(directory, "*.mp4");

                VideoURIs.AddRange(files);
                Plugin.Logger.LogInfo($"{nameof(AudioVideoManager)}::{nameof(LoadVideoURIs)}: found {files.Length} videos in the {directory} path.");
            } catch (Exception e) {
                Plugin.Logger.LogInfo($"{nameof(AudioVideoManager)}::{nameof(LoadVideoURIs)}(): failed to load files, exception: {e.Message}");
                Plugin.Logger.LogInfo($"--STACK--{Environment.NewLine}{e.StackTrace}");

            }
        }

        /// <summary>
        /// Loads the next video that'll play on the television.
        /// </summary>
        /// <param name="instance"><see cref="TVScript"/> instance.</param>
        /// <param name="currentVideoIndex">Which custom video is currently playing.</param>
        public static void PrepareVideo(TVScript instance, int currentVideoIndex) {
            if (currentVideoIndex < 0)
                return;

            string tag = $"{nameof(AudioVideoManager)}::{nameof(NextTVVideoPlayer)}()";
            Plugin.Logger.LogDebug(tag);

            if (NextTVVideoPlayer != null && NextTVVideoPlayer.gameObject.activeInHierarchy)
                UnityEngine.Object.Destroy(NextTVVideoPlayer);

            NextTVVideoPlayer = instance.gameObject.AddComponent<VideoPlayer>();
            NextTVVideoPlayer.playOnAwake = false;
            NextTVVideoPlayer.isLooping = false;
            NextTVVideoPlayer.source = VideoSource.Url;
            NextTVVideoPlayer.controlledAudioTrackCount = 1;
            NextTVVideoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            NextTVVideoPlayer.SetTargetAudioSource(0, instance.tvSFX);
            NextTVVideoPlayer.url = "file://" + VideoURIs[currentVideoIndex % VideoURIs.Count];
            NextTVVideoPlayer.Prepare();
            NextTVVideoPlayer.prepareCompleted += delegate {
                Plugin.Logger.LogDebug($"{tag}#prepareCompleted!");
            };
        }
        #endregion

        #region Audio
        public enum AudioPlayerItemType {
            Boombox,
            RecordPlayer
        }

        public static void VolumeUp(bool updateAllAudioSources = false) {
            updateAllAudioSources = updateAllAudioSources || !VolumeControllers.Any(x => x.BeingLookedAtByPlayer);
            Plugin.Logger.LogDebug($"{nameof(AudioVideoManager)}::{nameof(VolumeUp)}(updateAllAudioSources: {updateAllAudioSources})");

            foreach (var volumeController in VolumeControllers) {
                if (updateAllAudioSources)
                    volumeController.VolumeUp();
                else if (volumeController.BeingLookedAtByPlayer)
                    volumeController.VolumeUp();
            }
        }
        public static void VolumeDown(bool updateAllAudioSources = false) {
            updateAllAudioSources = updateAllAudioSources || !VolumeControllers.Any(x => x.BeingLookedAtByPlayer);
            Plugin.Logger.LogDebug($"{nameof(AudioVideoManager)}::{nameof(VolumeDown)}(updateAllAudioSources: {updateAllAudioSources})");

            foreach (var volumeController in VolumeControllers) {
                if (updateAllAudioSources)
                    volumeController.VolumeDown();
                else if (volumeController.BeingLookedAtByPlayer)
                    volumeController.VolumeDown();
            }
        }
        public static void UpdateVolumes() {
            VolumeControllers.ForEach(x => x.UpdateVolumes());
        }
        public static void UpdateTooltip() {
            VolumeControllers.ForEach(x => x.UpdateTooltip());
        }


        /// <summary>
        /// Loads the custom audio file URIs and loads all the audio clips.
        /// </summary>
        public static void LoadAudioClips() {
            LoadSongURIs();
            PrepareBoomboxClips();
            PrepareRecordPlayerClips();
        }

        /// <summary>
        /// Loads the custom audio file URIs.
        /// </summary>
        public static void LoadSongURIs() {
            try {
                var globalAudioDirectory = Path.Join(Plugin.PluginDirectory, "AudioClips");
                var boomboxAudioDirectory = Path.Join(Plugin.PluginDirectory, "Boombox_AudioClips");
                var recordAudioDirectory = Path.Join(Plugin.PluginDirectory, "RecordPlayer_AudioClips");

                string[] globalAudioFiles = [];
                if (Directory.Exists(globalAudioDirectory))
                    globalAudioFiles = Directory.GetFiles(globalAudioDirectory);

                string[] boomboxAudioFiles = [];
                if (Directory.Exists(boomboxAudioDirectory))
                    boomboxAudioFiles = Directory.GetFiles(boomboxAudioDirectory);

                string[] recordAudioFiles = [];
                if (Directory.Exists(recordAudioDirectory))
                    recordAudioFiles = Directory.GetFiles(recordAudioDirectory);


                // only audio files
                globalAudioFiles = globalAudioFiles.Where(IsAudioFile).ToArray();
                boomboxAudioFiles = boomboxAudioFiles.Where(IsAudioFile).ToArray();
                recordAudioFiles = recordAudioFiles.Where(IsAudioFile).ToArray();

                AudioURIs = new List<string>(globalAudioFiles);
                BoomboxOnlyAudioURIs = new List<string>(boomboxAudioFiles);
                RecordPlayerOnlyAudioURIs = new List<string>(recordAudioFiles);


                Plugin.Logger.LogInfo(
                    $"{nameof(AudioVideoManager)}::{nameof(LoadSongURIs)}(): "
                    + Environment.NewLine + $"found {AudioURIs.Count} global audio clips in {globalAudioDirectory} path, "
                    + Environment.NewLine + $"{BoomboxOnlyAudioURIs.Count} boombox unique audio clips in {boomboxAudioDirectory}, "
                    + Environment.NewLine + $"and {RecordPlayerOnlyAudioURIs.Count} record category unique audio clips in {recordAudioDirectory}."
                );
            } catch (Exception e) {
                Plugin.Logger.LogError($"{nameof(AudioVideoManager)}::{nameof(LoadSongURIs)}(): failed to load song URIs. Exception: {e.Message}");
                Plugin.Logger.LogDebug("--STACK--" + Environment.NewLine + e.StackTrace);
            }
        }


        /// <summary>
        /// Gets the <see cref="AudioType"/> of an audio given the file's extension.
        /// </summary>
        /// <param name="file">Path to the audio file.</param>
        /// <returns>Type of audio file based off of the file extension.</returns>
        public static AudioType GetAudioType(string file) {
            if (!File.Exists(file))
                return AudioType.UNKNOWN;

            string extension = Path.GetExtension(file)?.ToLowerInvariant() ?? string.Empty;

            return extension switch {
                ".mp3" => AudioType.MPEG,
                ".wav" => AudioType.WAV,
                ".ogg" => AudioType.OGGVORBIS,
                ".aif" or ".aiff" => AudioType.AIFF,
                _ => AudioType.UNKNOWN,
            };
        }

        /// <summary>
        /// Check whether a file is known audio file type.
        /// </summary>
        /// <param name="file">Path to the audio file.</param>
        /// <returns>Whether the file is a valid audio file.</returns>
        public static bool IsAudioFile(string file) => GetAudioType(file) != AudioType.UNKNOWN;


        /// <summary>
        /// Prepares the audio clips for a given <see cref="AudioPlayerItemType"/>.
        /// </summary>
        /// <param name="category">The audio category you're preparing the clips for (boombox, for example).</param>
        public static void PrepareClips(AudioPlayerItemType category) {
            List<string> audioURIs = [];
            if (category == AudioPlayerItemType.Boombox) {
                // is this even allowed and are we not currently loading them?
                if (PluginConfig.BoomboxAudioMode == Plugin.AudioMode.NoCustomAudio || _loadingBoomboxAudioClips) return;

                _loadingBoomboxAudioClips = true;

                audioURIs = BoomboxAudioURIs;
                if (BoomboxAudioClips.Count > 0)
                    return;

            } else if (category == AudioPlayerItemType.RecordPlayer) {
                if (PluginConfig.RecordPlayerAudioMode == Plugin.AudioMode.NoCustomAudio || _loadingRecordPlayerAudioClips)
                    return;
                _loadingRecordPlayerAudioClips = true;

                audioURIs = RecordPlayerAudioURIs;
                if (RecordPlayerClips.Count > 0)
                    return;
            }

            Plugin.Logger.LogInfo($"Preparing {category} audio clips...");
            var coroutines = new List<Coroutine>();
            foreach (string audioURI in audioURIs)
                coroutines.Add(AudioCoroutineStarter.StartCoroutine(LoadAudioClip(audioURI, category)));
            AudioCoroutineStarter.StartCoroutine(WaitForAllAudioClips(coroutines, category));
        }

        /// <summary>
        /// Populates the <see cref="BoomboxAudioClips"/> list.
        /// </summary>
        /// <remarks>
        /// This is not as simple as loading a bunch of VideoPlayers.
        /// </remarks>
        public static void PrepareBoomboxClips() => PrepareClips(AudioPlayerItemType.Boombox);

        /// <summary>
        /// Populates the <see cref="RecordPlayerClips"/> list.
        /// </summary>
        /// <remarks>
        /// This is not as simple as loading a bunch of VideoPlayers.
        /// </remarks>
        public static void PrepareRecordPlayerClips() {
            copiedGameRecordPlayerClips = false;
            PrepareClips(AudioPlayerItemType.RecordPlayer);
        }

        /// <summary>
        /// Sets a <see cref="BoomboxItem"/>'s audio clips.
        /// </summary>
        /// <param name="boombox">BoomboxItem instance.</param>
        public static void ApplyClipsToBoombox(BoomboxItem boombox) {
            switch (PluginConfig.BoomboxAudioMode) {
                case Plugin.AudioMode.NoCustomAudio:
                    // how'd we get here??
                    return;
                case Plugin.AudioMode.InterweaveCustomAudio:
                    boombox.musicAudios = [.. boombox.musicAudios, .. BoomboxAudioClips];
                    break;
                case Plugin.AudioMode.OnlyCustomAudio:
                    boombox.musicAudios = [.. BoomboxAudioClips];
                    break;
            }

            Plugin.Logger.LogDebug($"{nameof(AudioVideoManager)}::{nameof(ApplyClipsToBoombox)}(): boombox now has {boombox.musicAudios.Length} clips.");
        }

        /// <summary>
        /// Sets the record player's audio clips. Called when the record first starts playing.
        /// </summary>
        public static void PlayARecordPlayerClip() {
            string tag = $"{nameof(AudioVideoManager)}::{nameof(PlayARecordPlayerClip)}()";

            var animatedObjectTriggers = UnityEngine.Object.FindObjectsOfType<AnimatedObjectTrigger>();
            AnimatedObjectTrigger? recordPlayer = null;
            foreach (var trigger in animatedObjectTriggers) {
                if (!trigger.animationString.Equals("playanim", StringComparison.OrdinalIgnoreCase))
                    continue;

                recordPlayer = trigger;
                break;
            }

            if (recordPlayer == null) {
                Plugin.Logger.LogInfo($"{tag}: Unable to find record player!?");
                return;
            }

            try {
                bool hasSetupRecordPlayerAudioController = false;
                var interactTrigger = recordPlayer.GetComponentInChildren<InteractTrigger>();
                foreach (var volumeController in VolumeControllers) {
                    if (volumeController == null || volumeController.GetType() != typeof(VolumeControllerFurniture))
                        continue;

                    if (((VolumeControllerFurniture)volumeController).Trigger == interactTrigger) {
                        hasSetupRecordPlayerAudioController = true;
                        break;
                    }
                }

                if (!hasSetupRecordPlayerAudioController)
                    VolumeControllers.Add(new VolumeControllerFurniture(recordPlayer));
            } catch (Exception e) {
                Plugin.Logger.LogWarning($"{tag}: Unable to check if the record player has already been add to the volume controllers. (Exception thrown: {e.Message}).");
                Plugin.Logger.LogDebug(e.StackTrace);
            }


            switch (PluginConfig.RecordPlayerAudioMode) {
                case Plugin.AudioMode.NoCustomAudio:
                    // how'd we get here??
                    return;
                case Plugin.AudioMode.InterweaveCustomAudio:
                    if (!copiedGameRecordPlayerClips) {
                        RecordPlayerClips.Add(recordPlayer.playWhileTrue);
                        copiedGameRecordPlayerClips = true;
                    }

                    recordPlayer.playWhileTrue = RecordPlayerClips[UnityEngine.Random.Range(0, RecordPlayerClips.Count)];
                    break;
                case Plugin.AudioMode.OnlyCustomAudio:
                    // pick random record
                    recordPlayer.playWhileTrue = RecordPlayerClips[UnityEngine.Random.Range(0, RecordPlayerClips.Count)];
                    break;
            }

            Plugin.Logger.LogDebug($"{nameof(AudioVideoManager)}::{nameof(PlayARecordPlayerClip)}(): record player now playing {recordPlayer.playWhileTrue.name}.");
        }

        private static AudioClip CloneAudioClip(AudioClip audioClip) {
            var newAudioClip = AudioClip.Create(audioClip.name + "_cloned", audioClip.samples, audioClip.channels, audioClip.frequency, false);
            float[] copyData = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(copyData, 0);
            newAudioClip.SetData(copyData, 0);
            return newAudioClip;
        }


        private static IEnumerator LoadAudioClip(string uri, AudioPlayerItemType category) {
            string tag = $"{nameof(AudioVideoManager)}::{nameof(LoadAudioClip)}(uri: {uri})";
            Plugin.Logger.LogDebug(tag);

            var audioType = GetAudioType(uri);
            if (audioType == AudioType.UNKNOWN) {
                Plugin.Logger.LogWarning($"{tag}: failed to load audio clip (unsupported/unknown file type).");
                yield break;
            }

            var audioLoader = UnityWebRequestMultimedia.GetAudioClip(uri, audioType);
            if (PluginConfig.StreamCustomAudioFromStorage)
                ((DownloadHandlerAudioClip)audioLoader.downloadHandler).streamAudio = true;

            audioLoader.SendWebRequest();
            // wait for it to load...
            while (true) {
                if (audioLoader.isDone)
                    break;
                yield return null;
            }

            if (!string.IsNullOrEmpty(audioLoader.error)) {
                Plugin.Logger.LogError($"{tag}: failed to load audio clip. Error: {audioLoader.error}");
                yield break;
            }

            var audioClip = DownloadHandlerAudioClip.GetContent(audioLoader);
            // okay cool where's my clip?
            if (audioClip == null) {
                Plugin.Logger.LogError($"{tag}: audioClip null, clearly it didn't load. Not sure why, possible mismatch between file type and extension?");
                yield break;
            }

            // limbo
            if (audioClip.loadState != AudioDataLoadState.Loaded) {
                Plugin.Logger.LogError($"{tag}: audioClip not in \"loaded\" state, stuck in state: \"{audioClip.loadState}\"?");
                yield break;
            }

            Plugin.Logger.LogDebug($"{tag}: loaded audio clip!");
            audioClip.name = Path.GetFileName(uri);
            if (category == AudioPlayerItemType.Boombox)
                BoomboxAudioClips.Add(audioClip);
            else if (category == AudioPlayerItemType.RecordPlayer)
                RecordPlayerClips.Add(audioClip);
            yield break;
        }

        private static IEnumerator WaitForAllAudioClips(List<Coroutine> loadAudioClipCoroutines, AudioPlayerItemType category) {
            foreach (var coroutine in loadAudioClipCoroutines)
                yield return coroutine;

            if (category == AudioPlayerItemType.Boombox) {
                BoomboxAudioClips.Sort((x, y) => x.name.CompareTo(y.name));
                _loadingBoomboxAudioClips = false;
                OnAllBoomboxAudioClipsLoaded?.Invoke();
                OnAllBoomboxAudioClipsLoaded = null;
            } else if (category == AudioPlayerItemType.RecordPlayer) {
                RecordPlayerClips.Sort((x, y) => x.name.CompareTo(y.name));
                _loadingRecordPlayerAudioClips = false;
                OnAllRecordPlayerAudioClipsLoaded?.Invoke();
                OnAllRecordPlayerAudioClipsLoaded = null;
            }
        }


        // What the hell
        internal class AudioCoroutineStarter : MonoBehaviour {
            private static AudioCoroutineStarter? _instance;
            public static new Coroutine StartCoroutine(IEnumerator routine) {
                if (_instance == null) {
                    _instance = new GameObject(nameof(AudioCoroutineStarter)).AddComponent<AudioCoroutineStarter>();
                    DontDestroyOnLoad(_instance);
                }
                return ((MonoBehaviour)_instance).StartCoroutine(routine);
            }
        }

        #endregion

    }    
}
