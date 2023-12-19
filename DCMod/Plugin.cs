using BepInEx;
using BepInEx.Logging;
using DCMod.Patches;
using HarmonyLib;
using System.IO;
using System.Reflection;

namespace DCMod;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInProcess("Lethal Company.exe")]
[BepInProcess("LethalCompany.exe")]
public class Plugin : BaseUnityPlugin {
    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    internal static new ManualLogSource Logger;
    #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    internal static Harmony Harmony = new Harmony(PluginInfo.PLUGIN_GUID);

    internal static string PluginDirectory {
        get => Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
    }

    #region Enums
    public enum ScrapProtection {
        None,
        RandomChance,
        All,
    }

    public enum TelevisionVideoMode {
        NoCustomVideos,
        //InterweaveCustomVideos,
        //HelmetCameras,
        OnlyCustomVideos
    }

    public enum AudioMode {
        NoCustomAudio,
        InterweaveCustomAudio,
        OnlyCustomAudio,
    }
    #endregion

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_NAME} (version {PluginInfo.PLUGIN_VERSION}) loading.");

        // everything's static.
        _ = new PluginConfig(Config);

        // patch stuff, individual methods
        Harmony.PatchAll(typeof(GameNetworkManagerPatch));
        Harmony.PatchAll(typeof(RoundManagerPatch));
        Harmony.PatchAll(typeof(StartOfRoundPatch));
        Harmony.PatchAll(typeof(TVScriptPatches));
        Harmony.PatchAll(typeof(BoomboxItemPatch));
        Harmony.PatchAll(typeof(AnimatedObjectTriggerPatch));
        Harmony.PatchAll(typeof(PlayerControllerBPatch));

        // Modify existing game methods (IL editors)
        Harmony.PatchAll(typeof(BoomboxItemPocketItem));
        Harmony.PatchAll(typeof(ResetSavedGameValues));

        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_NAME} has loaded.");
    }
}