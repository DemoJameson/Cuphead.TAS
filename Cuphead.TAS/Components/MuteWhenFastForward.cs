using HarmonyLib;
using TAS;
using UnityEngine;

namespace CupheadTAS.Components;

[HarmonyPatch]
public class MuteInBackground : PluginComponent {
    private float? originalVolume;

    private void Update() {
        if (Manager.FastForwarding && !originalVolume.HasValue) {
            originalVolume = AudioListener.volume;
            AudioListener.volume = 0;
        } else if (!Manager.FastForwarding && originalVolume.HasValue) {
            AudioListener.volume = originalVolume.Value;
            originalVolume = null;
        }
    }

    [HarmonyPatch(typeof(AudioManagerComponent), nameof(AudioManagerComponent.OnPlay))]
    [HarmonyPrefix]
    private static bool AudioManagerComponentOnPlay(string key) {
        if (Manager.FastForwarding) {
            string lowerKey = key.ToLower();
            return !lowerKey.Contains("begin") && !lowerKey.Contains("intro") && !lowerKey.Contains("ready") && lowerKey.Contains("start");
        } else {
            return true;
        }
    }
}