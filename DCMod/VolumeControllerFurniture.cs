using System;
using UnityEngine;

namespace DCMod {

    /// <summary>
    /// Represents the television and record player.
    /// </summary>
    public class VolumeControllerFurniture : VolumeController {
        /// <summary>
        /// Item this controls.
        /// </summary>
        public InteractTrigger Trigger { get; private set; }

        /// <summary>
        /// Creates a new instance of <see cref="VolumeControllerFurniture"/> for (likely) the record player.
        /// </summary>
        /// <param name="instance">Record player to control.</param>
        /// <exception cref="NullReferenceException">If the <see cref="AudioSource"/> or <see cref="InteractTrigger"/> for the instance is <see langword="null"/>.</exception>
        public VolumeControllerFurniture(AnimatedObjectTrigger instance) : base("RecordPlayer") {
            var interactTrigger = instance.GetComponentInChildren<InteractTrigger>();

            string tag = $"{nameof(VolumeControllerGrabbable)}::(audioSource, interactTrigger)";
            if (instance.thisAudioSource == null || interactTrigger == null)
                throw new NullReferenceException($"{tag}: someone tried passing in a null {(instance.thisAudioSource == null ? "audio source" : "interactTrigger")} :/");

            OriginalTooltip = interactTrigger.hoverTip;

            Trigger = interactTrigger;
            AudioSource = instance.thisAudioSource;
            UpdateTooltip();
            UpdateVolumes();
        }

        /// <summary>
        /// Creates a new instance of <see cref="VolumeControllerFurniture"/> for a TV.
        /// </summary>
        /// <param name="instance">The TV to control.</param>
        /// <exception cref="NullReferenceException">If the <see cref="AudioSource"/> or <see cref="InteractTrigger"/> for the instance is <see langword="null"/>.</exception>
        public VolumeControllerFurniture(TVScript instance) : base("TV") {
            var interactTrigger = instance.transform.parent?.GetComponentInChildren<InteractTrigger>();
            if (instance.tvSFX == null || interactTrigger == null)
                throw new NullReferenceException($"{nameof(VolumeControllerGrabbable)}::(TVScript): someone tried passing in a null {(instance.tvSFX == null ? "audio source" : "interactTrigger")} :/");

            OriginalTooltip = interactTrigger.hoverTip;

            Trigger = interactTrigger;
            AudioSource = instance.tvSFX;
            UpdateTooltip();
            UpdateVolumes();
        }


        public override void UpdateTooltip() {
            if (Trigger != null)
                Trigger.hoverTip = CurrentToolTip;
        }
    }
}
