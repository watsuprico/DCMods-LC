using System;

namespace DCMod {
    /// <summary>
    /// Represents the boombox.
    /// </summary>
    public class VolumeControllerGrabbable : VolumeController {
        /// <summary>
        /// Item this controls.
        /// </summary>
        public GrabbableObject? Grabbable { get; private set; }

        /// <summary>
        /// Original text shown to the user when holding <see cref="Grabbable"/>.
        /// </summary>
        public string OriginalHoldTooltip = string.Empty;
        /// <summary>
        /// Text to be shown to the user when holding <see cref="Grabbable"/>.
        /// </summary>
        public string CurrentHoldToolTip {
            get => $"{OriginalHoldTooltip}{Environment.NewLine}{CurrentVolumePercentage}% volume{Environment.NewLine}Volume down [{PluginConfig.VolumeDownKeyString}]{Environment.NewLine}Volume up [{PluginConfig.VolumeUpKeyString}].";
        }

        /// <summary>
        /// Creates a new instance of <see cref="VolumeControllerGrabbable"/> using a <see cref="BoomboxItem"/> instance.
        /// </summary>
        /// <param name="instance"><see cref="BoomboxItem"/> to control.</param>
        public VolumeControllerGrabbable(BoomboxItem instance) : base(nameof(BoomboxItem)) {
            string tag = $"{nameof(VolumeControllerGrabbable)}::(BoomboxItem)";
            if (instance == null || instance.boomboxAudio == null) {
                Plugin.Logger.LogWarning($"{tag}: someone tried passing in a null {(instance == null ? "BoomboxItem" : "audio source")}... :/");
                return;
            }

            Plugin.Logger.LogInfo($"{tag}: Adding Boombox...");
            Grabbable = instance;
            AudioSource = instance.boomboxAudio;

            if (string.IsNullOrEmpty(OriginalTooltip))
                OriginalTooltip = instance.customGrabTooltip;
            if (string.IsNullOrEmpty(OriginalHoldTooltip))
                OriginalHoldTooltip = string.Join(Environment.NewLine, instance.itemProperties.toolTips);

            instance.itemProperties.canBeGrabbedBeforeGameStart = true;
            UpdateTooltip();
            UpdateVolumes();
        }

        public override void UpdateTooltip() {
            if (Grabbable == null)
                return;

            Grabbable.customGrabTooltip = CurrentToolTip;
            Grabbable.itemProperties.toolTips = new string[] { CurrentHoldToolTip };


            // If the player is holding an item, re-"activate" it to flash the tool tip
            var localPlayerController = GameNetworkManager.Instance.localPlayerController;

            if (localPlayerController != null && localPlayerController?.currentlyHeldObjectServer == Grabbable) localPlayerController.currentlyHeldObjectServer.EquipItem(); // requip it to refresh holdtip.
        }
    }
}
