using HarmonyLib;
using UnityEngine;
using Unity.Netcode;

namespace DCMod.Patches {
    public class RoundManagerPatch {
        [HarmonyPatch(typeof(RoundManager), "DespawnPropsAtEndOfRound")]
        [HarmonyPrefix]
        public static bool DespawnPropsAtEndOfRound(RoundManager __instance, bool despawnAllItems) {
            if (PluginConfig.ScrapProtection == Plugin.ScrapProtection.None || despawnAllItems)
                return true;
            if (!__instance.IsHost && !__instance.IsServer)
                return true;
            Plugin.Logger.LogDebug($"{nameof(DespawnPropsAtEndOfRound)}::(despawnAllItems: {despawnAllItems}) || All players dead? {StartOfRound.Instance.allPlayersDead}");
            if (!StartOfRound.Instance.allPlayersDead)
                return true;

            var grabbableObjects = Object.FindObjectsOfType<GrabbableObject>();
            foreach (var grabbableObject in grabbableObjects) {
                if (!grabbableObject.itemProperties.isScrap)
                    continue;

                if (grabbableObject.isInShipRoom && PluginConfig.CanKeepScrap()) {
                    int newWorth = (int)(grabbableObject.scrapValue * PluginConfig.ScrapValueModifierForAllPlayersDead);
                    Plugin.Logger.LogDebug($"Saving {grabbableObject.name} (worth {grabbableObject.scrapValue}  now worth {newWorth}).");
                    grabbableObject.SetScrapValue(newWorth);
                    continue;
                }

                grabbableObject.gameObject.GetComponent<NetworkObject>().Despawn();
                if (__instance.spawnedSyncedObjects.Contains(grabbableObject.gameObject)) __instance.spawnedSyncedObjects.Remove(grabbableObject.gameObject);
            }
            var temporaryObjects = GameObject.FindGameObjectsWithTag("TemporaryEffect");
            foreach (var temporaryObject in temporaryObjects)
                Object.Destroy(temporaryObject);

            return false;
        }
    }
}
