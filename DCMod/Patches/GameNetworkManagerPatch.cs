using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace DCMod.Patches {

    /// <summary>
    /// This is to patch <see cref="GameNetworkManager"/>
    /// </summary>
    public class GameNetworkManagerPatch {
        // This is for removing the items from the ship (?)
        [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.ResetUnlockablesListValues))]
        public static void ResetUnlockablesListValues() {
            Plugin.Logger.LogDebug($"{nameof(GameNetworkManagerPatch)}::{nameof(ResetUnlockablesListValues)}() hit");

            if (StartOfRound.Instance.unlockablesList.unlockables == null) return;

            for (int i = 0; i < StartOfRound.Instance.unlockablesList.unlockables.Count; i++) {
                var unlockable = StartOfRound.Instance.unlockablesList.unlockables[i];
                // check if it should be reset.
                if (PluginConfig.KeepUnlockable(i))
                    continue;

                unlockable.hasBeenUnlockedByPlayer = false;
                if (unlockable.unlockableType == 1) {
                    unlockable.placedPosition = Vector3.zero;
                    unlockable.placedRotation = Vector3.zero;
                    unlockable.hasBeenMoved = false;
                    unlockable.inStorage = false;
                }
            }
        }


        // This is for when the game ends and the saved data is reset
        // It's currently bugged.
        [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.ResetSavedGameValues))]
        [HarmonyPostfix]
        public static void ResetSavedGameValues() {
            Plugin.Logger.LogDebug($"{nameof(GameNetworkManagerPatch)}::{nameof(ResetSavedGameValues)}() hit");

            if (!ES3.KeyExists("UnlockedShipObjects", GameNetworkManager.Instance.currentSaveFileName))
                return;

            var saveFilePath = GameNetworkManager.Instance.currentSaveFileName;
            int[] unlockedShipObjects = ES3.Load<int[]>("UnlockedShipObjects", saveFilePath);
            unlockedShipObjects = unlockedShipObjects.Where(x => !PluginConfig.KeepUnlockable(x)).ToArray();
            ES3.Save("UnlockedShipObjects", unlockedShipObjects, saveFilePath);

            for (int i = 0; i < StartOfRound.Instance.unlockablesList.unlockables.Count; i++) {
                var unlockable = StartOfRound.Instance.unlockablesList.unlockables[i];

                // If suit or can keep -> continue
                if (unlockable.unlockableType == 0 || PluginConfig.KeepUnlockable(i))
                    continue;

                ES3.DeleteKey("ShipUnlockMoved_" + unlockable.unlockableName, saveFilePath);
                ES3.DeleteKey("ShipUnlockStored_" + unlockable.unlockableName, saveFilePath);
                ES3.DeleteKey("ShipUnlockPos_" + unlockable.unlockableName, saveFilePath);
                ES3.DeleteKey("ShipUnlockRot_" + unlockable.unlockableName, saveFilePath);
            }
        }
    }

    /// <summary>
    /// Modifies the game's <see cref="GameNetworkManager.ResetSavedGameValues"/> in runtime.
    /// </summary>
    /// <remarks>
    /// If you check the game's IL code, you'll see the original method deletes the saved key "UnlockedShipObjects", we want to keep that (probably).
    /// <c>
    /// ldstr "UnlockedShipObjects"
    /// call class GameNetworkManager GameNetworkManager::get_Instance()
    /// ldfld string GameNetworkManager::currentSaveFileName
    /// call void ['Assembly-CSharp-firstpass']ES3::DeleteKey(string, string)
    /// </c>
    /// We want to remove those lines, so we find the line containing the <c>ldstr "UnlockedShipObjects"</c> and remove it and the next 4 lines.
    /// 
    /// Additionally, it goes through each unlockable furniture and removes it's stored values ("ShipUnlockMoved_" for example).
    /// </remarks>
    [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.ResetSavedGameValues))]
    public class ResetSavedGameValues {
        private static readonly string DELETE_KEY_CALL_OPERAND = "void ['Assembly-CSharp-firstpass']ES3::DeleteKey(string, string)";
        private static readonly string UNLOCKED_SHIP_OBJECTS_KEY = "UnlockedShipObjects";
        private static readonly string UNLOCKED_SHIP_OBJECT_PREFIX = "ShipUnlock";
        private static readonly string UNLOCKED_SHIP_OBJECT_SUFIX = "_";

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            string tag = $"{nameof(ResetSavedGameValues)}::{nameof(Transpiler)}";
            Plugin.Logger.LogInfo($"{tag}()");

            var opcodes = new List<CodeInstruction>(instructions);
            int deleteKeyUnlockedShipObjectsStart = -1;
            int deleteKeyUnlockedShipObjectsEnd = -1;

            var deleteKeyUnlockDataStarts = new List<int>();
            var deleteKeyUnlockDataEnds = new List<int>();
            bool lookingForUnlockedShipObjectsDeleteKeyCall = false;
            bool lookingForUnlockDataDeleteKeyCall = false;


            for (int i = 0; i < opcodes.Count; i++) {
                var opcode = opcodes[i];
                var operand = string.Empty;
                try {
                    operand = opcode.operand.ToString();
                } catch { }

                if (opcode.opcode == OpCodes.Ldstr) {
                    if (operand.Equals(UNLOCKED_SHIP_OBJECTS_KEY, StringComparison.OrdinalIgnoreCase)) {
                        deleteKeyUnlockedShipObjectsStart = i;
                        Plugin.Logger.LogDebug($"{tag} >| found \"UnlockedShipObjects\" opcode at line {i}");
                        lookingForUnlockedShipObjectsDeleteKeyCall = true;

                    } else if (operand.StartsWith(UNLOCKED_SHIP_OBJECT_PREFIX, StringComparison.OrdinalIgnoreCase)
                              && operand.EndsWith(UNLOCKED_SHIP_OBJECT_SUFIX, StringComparison.OrdinalIgnoreCase)) {
                        deleteKeyUnlockDataStarts.Add(i);
                        Plugin.Logger.LogDebug($"{tag} >| found an unlock data delete opcode start at line {i}");
                        lookingForUnlockDataDeleteKeyCall = true;
                    }
                } else if (opcode.opcode == OpCodes.Call && operand.Equals(DELETE_KEY_CALL_OPERAND, StringComparison.OrdinalIgnoreCase)) {
                    if (lookingForUnlockedShipObjectsDeleteKeyCall) {
                        deleteKeyUnlockedShipObjectsEnd = i;
                        Plugin.Logger.LogDebug($"{tag} >| found ES3.DeleteKey(\"UnlockedShipObjects\", string) opcode at line {i}");
                        lookingForUnlockedShipObjectsDeleteKeyCall = false;

                    } else if (lookingForUnlockDataDeleteKeyCall) {
                        deleteKeyUnlockDataEnds.Add(i);
                        Plugin.Logger.LogDebug($"{tag} >| found an ES3.DeleteKey(\"ShipUnlock..._\", string) opcode end at line {i}");
                        lookingForUnlockDataDeleteKeyCall = false;
                    }
                }
            }

            // alright, hopefully we found our stuff
            for (int i = deleteKeyUnlockDataStarts.Count - 1; i >= 0; i--) {
                int start = deleteKeyUnlockDataStarts[i];
                int length = 11; // it was 11 lines on my machine..
                // ..but it could change for some reason, so find the end dYnaMicALlY
                if (deleteKeyUnlockDataStarts.Count == deleteKeyUnlockDataEnds.Count && deleteKeyUnlockDataEnds[i] > start) // makes sure the end is a valid(ish) value
                    length = deleteKeyUnlockDataEnds[i] - start;

                opcodes.RemoveRange(start, length);
                Plugin.Logger.LogDebug($"{tag} >| removed {length} opcodes ({start}-{start + length}) to prevent the ES3.DeleteKey(\"ShipUnlock..._\", string) method firing");
            }

            if (deleteKeyUnlockedShipObjectsStart >= 0) {
                int start = deleteKeyUnlockedShipObjectsStart;
                int length = 4;
                if (deleteKeyUnlockedShipObjectsEnd > deleteKeyUnlockedShipObjectsStart) length = deleteKeyUnlockedShipObjectsEnd - deleteKeyUnlockedShipObjectsStart;

                opcodes.RemoveRange(start, length);
                Plugin.Logger.LogDebug($"{tag} >| removed {length} opcodes ({start}-{start + length}) to prevent the ES3.DeleteKey(\"UnlockedShipObjects\", string) method firing");
            }

            return opcodes.AsEnumerable();
        }
    }
}
