using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Obeliskial_Essentials;

namespace VeteranHunterAndrin
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("com.stiffmeds.obeliskialessentials")]
    [BepInDependency("com.stiffmeds.obeliskialcontent")]
    [BepInProcess("AcrossTheObelisk.exe")]
    public class Plugin : BaseUnityPlugin
    {
        internal const int ModDate = 20250801;
        private readonly Harmony harmony = new(PluginInfo.PLUGIN_GUID);
        internal static ManualLogSource Log;
        private void Awake()
        {
            Log = Logger;
            Log.LogInfo($"{PluginInfo.PLUGIN_GUID} {PluginInfo.PLUGIN_VERSION} has loaded!");
            // register with Obeliskial Essentials
            Essentials.RegisterMod(
                _name: PluginInfo.PLUGIN_NAME,
                _author: "Shazixnar",
                _description: "Redone Andrin's traits.",
                _version: PluginInfo.PLUGIN_VERSION,
                _date: ModDate,
                _link: @"https://across-the-obelisk.thunderstore.io/package/Shazixnar/Veteran_Hunter_Andrin/",
                _contentFolder: "Veteran Hunter Andrin",
                _type: new string[5] { "content", "hero", "trait", "card", "perk" }
            );
            // apply patches
            harmony.PatchAll();
        }
    }
}
