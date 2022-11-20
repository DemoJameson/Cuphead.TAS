using BepInEx.Configuration;
using HarmonyLib;

namespace CupheadTAS.Components; 

[HarmonyPatch]
public class PreventSavingToDisk : PluginComponent {
    private static ConfigEntry<bool> preventSavingToDisk; 
    
    private void Awake() {
        preventSavingToDisk = Plugin.Instance.Config.Bind("General", "Prevent Saving To Disk", false);
    }

    [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.Save))]
    [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.SaveAll))]
    [HarmonyPrefix]
    private static bool PlayerDataSave() {
        return !preventSavingToDisk.Value;
    }
}