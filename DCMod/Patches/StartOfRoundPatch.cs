using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace DCMod.Patches {
    public class StartOfRoundPatch {
        [HarmonyPatch(typeof(StartOfRound), "Start")]
        [HarmonyPrefix]
        public static bool Start() {
            Plugin.Logger.LogInfo("StartOfRound Start");
            TimeOfDay.Instance.quotaVariables.deadlineDaysAmount = PluginConfig.DaysToMeetQuota;
            TimeOfDay.Instance.quotaVariables.startingCredits = 5000;

            AudioVideoManager.LoadAudioClips();
            AudioVideoManager.LoadVideoURIs();

            return true;
        }

        // I don't like it either, but eh whatever just override the damn thing
        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.ResetShip))]
        [HarmonyPrefix]
        public static bool ResetShipFull() {
            TimeOfDay.Instance.globalTime = 100f;
            TimeOfDay.Instance.profitQuota = TimeOfDay.Instance.quotaVariables.startingQuota;
            TimeOfDay.Instance.quotaFulfilled = 0;
            TimeOfDay.Instance.timesFulfilledQuota = 0;
            TimeOfDay.Instance.timeUntilDeadline = (int)(TimeOfDay.Instance.totalTime * TimeOfDay.Instance.quotaVariables.deadlineDaysAmount);
            TimeOfDay.Instance.UpdateProfitQuotaCurrentTime();
            StartOfRound.Instance.randomMapSeed++;
            Debug.Log("Reset ship 0");
            StartOfRound.Instance.companyBuyingRate = 0.3f;
            StartOfRound.Instance.ChangeLevel(StartOfRound.Instance.defaultPlanet);
            StartOfRound.Instance.ChangePlanet();
            StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();
            var terminal = Object.FindObjectOfType<Terminal>();
            if (terminal != null) terminal.groupCredits = TimeOfDay.Instance.quotaVariables.startingCredits;
            if (StartOfRound.Instance.IsServer) {
                for (int i = 0; i < StartOfRound.Instance.unlockablesList.unlockables.Count; i++) {
                    var unlockableItem = StartOfRound.Instance.unlockablesList.unlockables[i];
                    // if default, spawnPrefab or can keep, continue
                    if ((unlockableItem.alreadyUnlocked && !unlockableItem.spawnPrefab) || PluginConfig.KeepUnlockable(i)) continue;

                    if (!StartOfRound.Instance.SpawnedShipUnlockables.TryGetValue(i, out var gameObject))
                        StartOfRound.Instance.SpawnedShipUnlockables.Remove(i);
                    else {
                        StartOfRound.Instance.SpawnedShipUnlockables.Remove(i);
                        if (gameObject != null) {
                            var networkObject = gameObject.GetComponent<NetworkObject>();
                            if (networkObject != null && networkObject.IsSpawned)
                                networkObject.Despawn();
                        }
                    }
                }
                RoundManager.Instance.DespawnPropsAtEndOfRound(despawnAllItems: true);
                StartOfRound.Instance.closetLeftDoor.SetBoolOnClientOnly(setTo: false);
                StartOfRound.Instance.closetRightDoor.SetBoolOnClientOnly(setTo: false);
            }

            // reset positions
            var placeableShipObjects = Object.FindObjectsOfType<PlaceableShipObject>();
            foreach (var placeableShipObject in placeableShipObjects) {
                var unlockableID = placeableShipObject.unlockableID;
                var unlockableItem = StartOfRound.Instance.unlockablesList.unlockables[unlockableID];
                if (unlockableItem.spawnPrefab) {
                    // this is a purchase

                    if (PluginConfig.KeepUnlockable(unlockableID)) if (!PluginConfig.KeepUnlockablePosition(unlockableID)) unlockableItem.inStorage = true;
                    else {
                        unlockableItem.hasBeenUnlockedByPlayer = false;
                        unlockableItem.inStorage = false;
                    }

                    foreach (var componentInChild in placeableShipObject.parentObject.GetComponentsInChildren<Collider>()) {
                        componentInChild.enabled = false;
                    }

                    continue;
                }

                if (!PluginConfig.KeepUnlockablePosition(unlockableID)) {
                    if (unlockableItem.alreadyUnlocked) {
                        // Default item
                        unlockableItem.inStorage = false;
                        placeableShipObject.parentObject.disableObject = false;
                        ShipBuildModeManager.Instance.ResetShipObjectToDefaultPosition(placeableShipObject);
                        continue;
                    }

                    if (PluginConfig.KeepUnlockable(unlockableID)) {
                        unlockableItem.hasBeenUnlockedByPlayer = true;
                        unlockableItem.inStorage = true;
                    } else {
                        unlockableItem.hasBeenUnlockedByPlayer = false;
                        unlockableItem.inStorage = false;
                    }

                    placeableShipObject.parentObject.disableObject = true;
                    /*ShipBuildModeManager.Instance.StoreObjectServerRpc(
                        placeableShipObject.parentObject.GetComponent<NetworkObject>(),
                        (int)GameNetworkManager.Instance.localPlayerController.playerClientId
                    );*/

                }
            }

            Debug.Log("Going to reset unlockables list!");
            GameNetworkManager.Instance.ResetUnlockablesListValues();
            Debug.Log("Reset unlockables list!");
            for (int k = 0; k < StartOfRound.Instance.unlockablesList.unlockables.Count; k++) {
                Debug.Log($"Unlockable: {StartOfRound.Instance.unlockablesList.unlockables[k].unlockableName}, inStorage?: {StartOfRound.Instance.unlockablesList.unlockables[k].inStorage}");
            }
            for (int l = 0; l < StartOfRound.Instance.allPlayerScripts.Length; l++) {
                SoundManager.Instance.playerVoicePitchTargets[l] = 1f;
                StartOfRound.Instance.allPlayerScripts[l].ResetPlayerBloodObjects();
                UnlockableSuit.SwitchSuitForPlayer(StartOfRound.Instance.allPlayerScripts[l], 0);
            }
            Debug.Log("Reset ship D");
            TimeOfDay.Instance.OnDayChanged();

            return false;
        }
    }
}
