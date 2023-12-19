using System;
using UnityEngine;

namespace DCMod {
    public abstract class VolumeController : IVolumeController {
        public string Name;

        // Target volume.
        public float CurrentVolume;

        // Set automatically before we update any tool/hover tips.
        public string OriginalTooltip = string.Empty;

        /// <summary>
        /// The audio source of the item being managed.
        /// </summary>
        public AudioSource? AudioSource;

        /// <summary>
        /// Whether the item this <see cref="VolumeController"/> manages is being looked at.
        /// </summary>
        public bool BeingLookedAtByPlayer = false;

        /// <summary>
        /// The current audio volume %.
        /// </summary>
        public string CurrentVolumePercentage {
            get => Mathf.RoundToInt(CurrentVolume * 100f).ToString();
        }

        /// <summary>
        /// Current tooltip, combination of the <see cref="OriginalTooltip"/>, <see cref="CurrentVolumePercentage"/> and a note on volume controls.
        /// </summary>
        public string CurrentToolTip {
            get => $"{OriginalTooltip}{Environment.NewLine}{CurrentVolumePercentage}% volume{Environment.NewLine}Volume down [{PluginConfig.VolumeDownKeyString}]{Environment.NewLine}Volume up [{PluginConfig.VolumeUpKeyString}].";
        }


        public VolumeController(string name = "unknownController") {
            Plugin.Logger.LogDebug($"Adding a new {GetType()} with the name {name}...");
            Name = name;
            CurrentVolume = Math.Min(PluginConfig.DefaultVolume, PluginConfig.MaxVolume);
        }


        public void VolumeUp() {
            var newVolume = Math.Min(CurrentVolume + PluginConfig.VolumeIncrements, PluginConfig.MaxVolume);
            Plugin.Logger.LogDebug($"{nameof(VolumeController)}::{nameof(VolumeUp)}(): ({Name}) {CurrentVolume} -> {newVolume}");
            CurrentVolume = newVolume;
            UpdateVolumes();
            UpdateTooltip();
        }

        public void VolumeDown() {
            var newVolume = Math.Max(CurrentVolume - PluginConfig.VolumeIncrements, 0);
            Plugin.Logger.LogDebug($"{nameof(VolumeController)}::{nameof(VolumeDown)}(): ({Name}) {CurrentVolume} -> {newVolume}");
            CurrentVolume = newVolume;
            UpdateVolumes();
            UpdateTooltip();
        }

        public void UpdateVolumes() {
            if (AudioSource != null)
                AudioSource.volume = CurrentVolume;
        }

        public abstract void UpdateTooltip();
    }
}
