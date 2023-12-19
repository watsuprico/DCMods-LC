namespace DCMod {
    internal interface IVolumeController {
        /// <summary>
        /// Increases the volume by <see cref="PluginConfig.VolumeIncrements"/> until <see cref="PluginConfig.MaxVolume"/>.
        /// </summary>
        /// <param name="updateAllAudioSources">Whether to update all <see cref="VolumeController.AudioSources"/> or just the one being looked at.</param>
        public void VolumeUp();

        /// <summary>
        /// Decreases the volume by <see cref="PluginConfig.VolumeIncrements"/> until 0.
        /// </summary>
        /// <param name="updateAllAudioSources">Whether to update all <see cref="VolumeController.AudioSources"/> or just the one being looked at.</param>
        public void VolumeDown();

        /// <summary>
        /// Updates the <see cref="VolumeController.AudioSources"/> with the <see cref="VolumeController.CurrentVolume"/>.
        /// </summary>
        public void UpdateVolumes();

        /// <summary>
        /// Updates the tool/hover tip of the items to include the new volume level.
        /// </summary>
        public void UpdateTooltip();
    }
}
