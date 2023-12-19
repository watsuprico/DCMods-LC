using BepInEx.Configuration;
using System;
using System.Linq;
using UnityEngine.InputSystem;

namespace DCMod {
    public class PluginConfig {
        private static PluginConfig? Instance;

        private readonly ConfigFile _configFile;
        private readonly ConfigEntry<int> _daysToMeetQuota;

        private readonly ConfigEntry<Plugin.ScrapProtection>? _scrapProtection;
        private readonly ConfigEntry<double>? _scrapProtectionChance;
        private readonly ConfigEntry<double>? _scrapValueModifierForAllPlayersDead;

        private readonly ConfigEntry<int[]>? _suitIds;
        private readonly ConfigEntry<bool>? _resetSuits;

        private readonly ConfigEntry<int[]>? _furnitureIdsToRemove;
        private readonly ConfigEntry<bool>? _resetFurniturePurchases;
        private readonly ConfigEntry<int[]>? _furnitureIdsToReset;
        private readonly ConfigEntry<bool>? _resetFurniturePlacements;
        private readonly ConfigEntry<bool>? _doNotResetDefaultFurniturePlacements;

        private readonly ConfigEntry<int[]>? _shipUpgradeIds;
        private readonly ConfigEntry<bool>? _resetShipUpgrades;

        // Audio/Visual
        private readonly ConfigEntry<Plugin.TelevisionVideoMode>? _televisionVideoMode;
        private readonly ConfigEntry<Plugin.AudioMode>? _boomboxAudioMode;
        private readonly ConfigEntry<Plugin.AudioMode>? _recordPlayerAudioMode;
        private readonly ConfigEntry<bool>? _stopBoomboxIfPocketed;
        private readonly ConfigEntry<bool>? _streamCustomAudioFromStorage;

        private readonly ConfigEntry<Key>? _volumeDownKey;
        private readonly ConfigEntry<Key>? _volumeUpKey;
        private readonly ConfigEntry<Key>? _adjustAllItemsVolumeKey;
        private readonly ConfigEntry<float>? _defaultVolume;
        private readonly ConfigEntry<float>? _volumeIncrements;
        private readonly ConfigEntry<float>? _maxVolume;



        private static readonly int[] DefaultSuitIds = [0, 1, 2, 3];
        public static readonly int[] DefaultFurnitureIds = [7, 8, 11, 15, 16];
        private static readonly int[] DefaultShipUpgradeIds = [5, 18, 19];


        #region Public config properties

        #region Scrap protections
        /// <summary>
        /// Whether to remove all scrap on the ship when all team members die.
        /// </summary>
        public static Plugin.ScrapProtection ScrapProtection {
            get => Instance?._scrapProtection?.Value ?? Plugin.ScrapProtection.None;
        }

        /// <summary>
        /// Value between 0 (never protected) and 1 (always protected)
        /// </summary>
        public static double ScrapProtectionChance {
            get => Math.Max(Math.Min((Instance?._scrapProtectionChance?.Value ?? 0) / 100, 1), 0);
        }

        /// <summary>
        /// If all player die, each scrap's value is modified by this percentage.
        /// </summary>
        public static double ScrapValueModifierForAllPlayersDead {
            get => Math.Min(Math.Max(Instance?._scrapValueModifierForAllPlayersDead?.Value ?? 1, 0), 1); // Bound between 0 and 1.
        }
        #endregion


        /// <summary>
        /// Number of days that will pass until the profit quota is required to be met.
        /// </summary>
        public static int DaysToMeetQuota {
            get => Math.Max(Instance?._daysToMeetQuota?.Value ?? 3, 1);
        }


        #region Resets (game overs)
        /// <summary>
        /// Removes any suit purchases.
        /// </summary>
        public static bool ResetSuits {
            get => Instance?._resetSuits?.Value ?? true;
        }

        /// <summary>
        /// Removes any furniture purchases.
        /// </summary>
        public static bool ResetFurniturePurchases {
            get => Instance?._resetFurniturePurchases?.Value ?? true;
        }

        /// <summary>
        /// Resets furniture placements. New purchases, if not reset, are placed into storage.
        /// </summary>
        public static bool ResetFurniturePositions {
            get => Instance?._resetFurniturePlacements?.Value ?? true;
        }

        public static bool DoNotResetDefaultFurniturePositions {
            get => Instance?._doNotResetDefaultFurniturePlacements?.Value ?? false;
        }

        /// <summary>
        /// Resets non-cosmetic ship upgrades.
        /// </summary>
        public static bool ResetShipUpgrades {
            get => Instance?._resetShipUpgrades?.Value ?? true;
        }


        /// <summary>
        /// Ids representing suits.
        /// </summary>
        /// <remarks>
        /// When it comes to remove a particular item, if <see cref="ResetSuits"/> is
        /// <see langword="false"/> and we're asked to remove a given id that is inside this, we simply don't.
        /// </remarks>
        public static int[] SuitIds {
            get => Instance?._suitIds?.Value ?? DefaultSuitIds;
        }

        /// <summary>
        /// Ids representing the furniture to remove.
        /// </summary>
        public static int[] FurnitureIdsToRemove {
            get => Instance?._furnitureIdsToRemove?.Value ?? DefaultFurnitureIds;
        }

        /// <summary>
        /// Ids representing the furniture to reset the position of.
        /// </summary>
        /// <remarks>
        /// Typically includes the ship upgrades.
        /// </remarks>
        public static int[] FurnitureIdsToReset {
            get => Instance?._furnitureIdsToReset?.Value ?? [.. DefaultFurnitureIds, .. DefaultShipUpgradeIds];
        }

        /// <summary>
        /// Ids representing the ship upgrades to reset.
        /// </summary>
        public static int[] ShipUpgradeIds {
            get => Instance?._shipUpgradeIds?.Value ?? DefaultShipUpgradeIds;
        }
        #endregion


        #region Audio/Visual
        public static Plugin.TelevisionVideoMode TelevisionVideoMode {
            get => Instance?._televisionVideoMode?.Value ?? Plugin.TelevisionVideoMode.NoCustomVideos;
        }

        public static Plugin.AudioMode BoomboxAudioMode {
            get => Instance?._boomboxAudioMode?.Value ?? Plugin.AudioMode.NoCustomAudio;
        }

        public static Plugin.AudioMode RecordPlayerAudioMode {
            get => Instance?._recordPlayerAudioMode?.Value ?? Plugin.AudioMode.NoCustomAudio;
        }

        public static bool StreamCustomAudioFromStorage {
            get => Instance?._streamCustomAudioFromStorage?.Value ?? false;
        }

        /// <summary>
        /// Whether <see cref="BoomboxItem.PocketItem"/> stops the music or not.
        /// </summary>
        public static bool StopBoomboxIfPocketed {
            get => Instance?._stopBoomboxIfPocketed?.Value ?? true;
        }

        /// <summary>
        /// Key to press to turn the volume of <see cref="VolumeController"/> up.
        /// </summary>
        public static Key VolumeUpKey {
            get => Instance?._volumeUpKey?.Value ?? Key.Equals;
        }
        public static string VolumeUpKeyString {
            get => KeyHelpers.GetStringForKey(VolumeUpKey);
        }

        /// <summary>
        /// Key to press to turn the volume of <see cref="VolumeController"/> DOWN.
        /// </summary>
        public static Key VolumeDownKey {
            get => Instance?._volumeDownKey?.Value ?? Key.Minus;
        }
        public static string VolumeDownKeyString {
            get => KeyHelpers.GetStringForKey(VolumeDownKey);
        }

        /// <summary>
        /// Key to press to adjust the volume on all items regardless of what you're looking at.
        /// </summary>
        public static Key AdjustAllItemsVolumeKey {
            get => Instance?._adjustAllItemsVolumeKey?.Value ?? Key.RightAlt;
        }
        public static string AdjustAllItemsVolumeKeyString {
            get => KeyHelpers.GetStringForKey(AdjustAllItemsVolumeKey);
        }

        /// <summary>
        /// Becomes the <see cref="VolumeController.CurrentVolume"/> setting on instance creation.
        /// </summary>
        public static float DefaultVolume {
            get => Instance?._defaultVolume?.Value ?? 0.5f;
        }

        /// <summary>
        /// How much the volume goes up/down on change.
        /// </summary>
        /// <remarks>
        /// Will not go above .5.
        /// </remarks>
        public static float VolumeIncrements {
            get => Math.Min(Instance?._volumeIncrements?.Value ?? 0.1f, 0.25f);
        }

        /// <summary>
        /// How loud the volume is aloud to get.
        /// </summary>
        /// <remarks>
        /// Will not go above 2.
        /// </remarks>
        public static float MaxVolume {
            get => Math.Min(Instance?._maxVolume?.Value ?? 1f, 2);
        }
        #endregion

        #endregion


        #region Config descriptions
        private static readonly string RESET_SUITS_DESCRIPTION = "The game id's of suits to be reset.";
        //private static readonly string SUIT_IDS_DESCRIPTION = "The game id's of the suits to be reset.";
        //private static readonly string FURNITURE_IDS_TO_REMOVE_DESCRIPTION = "The game id's of cosmetic furniture to be REMOVED on game over.";
        //private static readonly string FURNITURE_IDS_TO_RESET_DESCRIPTION = "The game id's of cosmetic furniture to reset their positions on game over.";
        private static readonly string RESET_FURNITURE_PURCHASES_DESCRIPTION = "Removes cosmetics purchases (defined in FurnitureIdsToRemove) on game over.";
        private static readonly string RESET_FURNITURE_POSITIONS_DESCRIPTION = "Resets the positions of cosmetic and upgrade purchases (defined in FurnitureIdsToReset) on game over.";
        private static readonly string DO_NOT_RESET_DEFAULT_FURNITURE_DESCRIPTION = "Whether the default furniture (light switch, bed, terminal, cupboard) is reset on game over.";
        //private static readonly string SHIP_UPGRADE_IDS_DESCRIPTION = "The game id's of ship upgrades to be removed on game over (if ResetShipUpgrades is true).";
        private static readonly string RESET_SHIP_UPGRADES_DESCRIPTION = "Removes any ship upgrades (defined in ShipUpgradeIds) on game over.";

        private static readonly string DAYS_TO_MEET_QUOTA_DESCRIPTION = "Number of days before the profit quota is required to be met.";

        private static readonly string SCRAP_PROTECTION_DESCRIPTION = "Scrap protections for team deaths. 'All' to keep all scrap, 'RandomChance' to randomly keep some scrap or 'None' to remove all scrap.";
        private static readonly string SCRAP_PROTECTION_CHANCE_DESCRIPTION = "If using RandomChance for your ScrapProtection, this is the chance you'll keep it. 0 (no chance) - 100 (always).";
        private static readonly string SCRAP_VALUE_MODIFIER_FOR_ALL_PLAYERS_DEAD_DESCRIPTION = "Each scrap's value is worth this percent more/less if all players die. 1 means the scrap value is untouched. .5 means all scrap is worth half-as-much if all player die.";

        private static readonly string TELEVISION_VIDEO_MODE_DESCRIPTION = "Whether the TV plays your custom videos or the game's stock videos. Custom videos are stored in the \"TV_Videos\" folder.";
        private static readonly string BOOMBOX_AUDIO_MODE_DESCRIPTION = "Whether the boombox plays your custom audio clips or the game's stock audio clips. Custom audio clips are stored in the \"AudioClips\" and \"Boombox_AudioClips\" folders.";
        private static readonly string RECORD_PLAYER_AUDIO_MODE_DESCRIPTION = "Whether the record player plays your custom audio clips or the game's stock audio clips. Custom audio clips are stored in the \"AudioClips\" and \"RecordPlayer_AudioClips\" folders.";
        private static readonly string STOP_BOOMBOX_IF_POCKETED_DESCRIPTION = "Whether the boombox item stops playing if it gets pocketed.";
        private static readonly string STREAM_CUSTOM_AUDIO_FROM_STORAGE_DESCRIPTION = "Uses less resources and is quicker, however, it prevents you from playing the same song twice at once.";

        private static readonly string VOLUME_UP_KEY_DESCRIPTION = "Key to press to turn the boombox, television and record player volumes up. See: https://docs.unity3d.com/Packages/com.unity.inputsystem@0.2/api/UnityEngine.InputSystem.Key.html";
        private static readonly string VOLUME_DOWN_KEY_DESCRIPTION = "Key to press to turn the boombox, television and record player volumes DOWN. See: https://docs.unity3d.com/Packages/com.unity.inputsystem@0.2/api/UnityEngine.InputSystem.Key.html";
        private static readonly string ADJUST_ALL_ITEMS_VOLUME_KEY_DESCRIPTION = "Key to press to adjust the volume for all items. By default, only the volume of the item you're looking at will adjust. Press this key to adjust all items, regardless if you're looking at them or not.";
        private static readonly string VOLUME_INCREMENTS_DESCRIPTION = "How much the volume changes when the volume up/down keys are pressed.";
        private static readonly string DEFAULT_VOLUME_DESCRIPTION = "Uh, might set the default volume of players on game boot, but don't quote me on that.";
        private static readonly string MAX_VOLUME_DESCRIPTION = "The maximum the volume can go to. 0-1, absolute max is 2. Don't do 2. Don't even go about 1, actually.";
        #endregion


        public PluginConfig(ConfigFile config) {
            _configFile = config;
            Instance = this;

            //_suitIds = config.Bind("ItemIds", "ItemIds", DefaultSuitIds, SUIT_IDS_DESCRIPTION);
            //_furnitureIdsToRemove = config.Bind("ItemIds", "FurnitureIdsToRemove", DefaultFurnitureIds, FURNITURE_IDS_TO_REMOVE_DESCRIPTION);
            //_furnitureIdsToReset = config.Bind("ItemIds", "FurnitureIdsToReset", DefaultFurnitureIds.Concat(ShipUpgradeIds).ToArray(), FURNITURE_IDS_TO_RESET_DESCRIPTION);
            //_shipUpgradeIds = config.Bind("ItemIds", "ShipUpgradeIds", DefaultShipUpgradeIds, SHIP_UPGRADE_IDS_DESCRIPTION);
            _resetSuits = config.Bind("QuotaTweaks", "ResetSuits", true, RESET_SUITS_DESCRIPTION);
            _resetFurniturePurchases = config.Bind("QuotaTweaks", "ResetFurniturePurchases", true, RESET_FURNITURE_PURCHASES_DESCRIPTION);
            _resetFurniturePlacements = config.Bind("QuotaTweaks", "ResetFurniturePositions", true, RESET_FURNITURE_POSITIONS_DESCRIPTION);
            _doNotResetDefaultFurniturePlacements = config.Bind("QuotaTweaks", "DoNotResetDefaultFurniturePlacements", false, DO_NOT_RESET_DEFAULT_FURNITURE_DESCRIPTION);
            _resetShipUpgrades = config.Bind("QuotaTweaks", "ResetShipUpgrades", true, RESET_SHIP_UPGRADES_DESCRIPTION);

            _daysToMeetQuota = config.Bind("QuotaTweaks", "DaysToMeetQuota", 4, DAYS_TO_MEET_QUOTA_DESCRIPTION);

            _scrapProtection = config.Bind("DayEnds", "ScrapProtection", Plugin.ScrapProtection.None, SCRAP_PROTECTION_DESCRIPTION);
            _scrapProtectionChance = config.Bind("DayEnds", "ScrapProtectionChance", 49.0, SCRAP_PROTECTION_CHANCE_DESCRIPTION);
            _scrapValueModifierForAllPlayersDead = config.Bind("DayEnds", "ScrapValueModifierForAllPlayersDead", 1.0, SCRAP_VALUE_MODIFIER_FOR_ALL_PLAYERS_DEAD_DESCRIPTION);


            _televisionVideoMode = config.Bind("Video", "TelevisionVideoMode", Plugin.TelevisionVideoMode.NoCustomVideos, TELEVISION_VIDEO_MODE_DESCRIPTION);
            _recordPlayerAudioMode = config.Bind("Audio", "RecordPlayerAudioMode", Plugin.AudioMode.NoCustomAudio, RECORD_PLAYER_AUDIO_MODE_DESCRIPTION);
            _boomboxAudioMode = config.Bind("Audio", "BoomboxAudioMode", Plugin.AudioMode.NoCustomAudio, BOOMBOX_AUDIO_MODE_DESCRIPTION);
            _stopBoomboxIfPocketed = config.Bind("Audio", "StopBoomboxIfPocketed", true, STOP_BOOMBOX_IF_POCKETED_DESCRIPTION);
            _streamCustomAudioFromStorage = config.Bind("Audio", "StreamCustomAudioFromStorage", false, STREAM_CUSTOM_AUDIO_FROM_STORAGE_DESCRIPTION);

            _volumeUpKey = config.Bind("Audio", "VolumeUpKey", Key.Equals, VOLUME_UP_KEY_DESCRIPTION);
            _volumeDownKey = config.Bind("Audio", "VolumeDownKey", Key.Minus, VOLUME_DOWN_KEY_DESCRIPTION);
            _adjustAllItemsVolumeKey = config.Bind("Audio", "AdjustAllItemsVolumeKey", Key.RightAlt, ADJUST_ALL_ITEMS_VOLUME_KEY_DESCRIPTION);
            _volumeIncrements = config.Bind("Audio", "VolumeIncrements", 0.05f, VOLUME_INCREMENTS_DESCRIPTION);
            _defaultVolume = config.Bind("Audio", "DefaultVolume", 0.6f, DEFAULT_VOLUME_DESCRIPTION);
            _maxVolume = config.Bind("Audio", "MaxVolume", 1f, MAX_VOLUME_DESCRIPTION);



            Plugin.Logger.LogInfo("Plugin configuration setup successful.");
        }

        /// <summary>
        /// Whether this scrap piece can be kept.
        /// </summary>
        /// <returns>If this scrap can be saved.</returns>
        public static bool CanKeepScrap() {
            return ScrapProtection switch {
                Plugin.ScrapProtection.All => true,
                Plugin.ScrapProtection.RandomChance => new Random().NextDouble() < ScrapProtectionChance,
                Plugin.ScrapProtection.None => false,
                _ => false,
            };
        }

        /// <summary>
        /// Whether this item can keep it's position.
        /// </summary>
        /// <param name="itemCode">Item to check.</param>
        /// <returns>If the item does not need to reset its position.</returns>
        public static bool KeepUnlockablePosition(int itemCode) {
            if (DoNotResetDefaultFurniturePositions && DefaultFurnitureIds.Contains(itemCode))
                return true;
            if (ResetFurniturePositions && FurnitureIdsToReset.Contains(itemCode))
                return false;
            return true;
        }

        /// <summary>
        /// Whether this item can be kept after missing the quota.
        /// </summary>
        /// <param name="itemCode">Item to check.</param>
        /// <returns>If the item can be kept after missing the quota.</returns>
        public static bool KeepUnlockable(int itemCode) {
            if (SuitIds.Contains(itemCode) && ResetSuits) {
                return false;
            }
            if (FurnitureIdsToRemove.Contains(itemCode) && ResetFurniturePurchases) {
                return false;
            }

            Plugin.Logger.LogInfo($"Keeping unlockable {itemCode}");
            return true;
        }
    }
}
