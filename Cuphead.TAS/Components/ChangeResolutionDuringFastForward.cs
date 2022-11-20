using BepInEx.Configuration;
using HarmonyLib;
using TAS;
using UnityEngine;

namespace CupheadTAS.Components;

[HarmonyPatch]
public class ChangeResolutionDuringFastForward : PluginComponent {
    private static ConfigEntry<bool> changeResolutionDuringFastForward;
    private static Resolution? lastResolution;

    [DisableRun]
    private static void ResetRenderers() {
        if (lastResolution is { } r) {
            Screen.SetResolution(r.width, r.height, SettingsData.Data.fullScreen, 0);
            lastResolution = null;
        }
    }

    private void Awake() {
        changeResolutionDuringFastForward = Plugin.Instance.Config.Bind("FastForward", "Change Resolution During FastForward", false);
    }

    private void Update() {
        if (!changeResolutionDuringFastForward.Value) {
            return;
        }

        if (Manager.UltraFastForwarding && lastResolution == null) {
            lastResolution = new Resolution {
                width = Screen.width,
                height = Screen.height
            };

            Screen.SetResolution(320, 200, Screen.fullScreen, 0);
        } else if (!Manager.UltraFastForwarding && lastResolution.HasValue) {
            ResetRenderers();
        }
    }
}