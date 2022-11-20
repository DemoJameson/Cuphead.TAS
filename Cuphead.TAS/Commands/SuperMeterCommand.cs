using TAS.Core.Input.Commands;
using TAS.Core.Utils;
using UnityEngine;

namespace CupheadTAS.Commands;

public class SuperMeterCommand {
    [TasCommand("SuperMeter", LegalInMainGame = false)]
    private static void SetSuperMeter(string[] args) {
        if (args.IsEmpty()) {
            return;
        }

        if (float.TryParse(args[0], out float superMeter)) {
            if (Object.FindObjectOfType<PlayerStatsManager>() is {} stats) {
                stats.SuperMeter = Mathf.Clamp(superMeter, 0, 50);
            }
        }
    }
}