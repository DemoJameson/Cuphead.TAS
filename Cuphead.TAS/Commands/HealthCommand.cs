using TAS.Core.Input.Commands;
using TAS.Core.Utils;
using UnityEngine;

namespace CupheadTAS.Commands;

public class HealthCommand {
    [TasCommand("Health", LegalInMainGame = false)]
    private static void Health(string[] args) {
        if (args.IsEmpty()) {
            return;
        }

        if (int.TryParse(args[0], out int health)) {
            if (Object.FindObjectOfType<PlayerStatsManager>() is {} stats) {
                stats.SetHealth(Mathf.Clamp(health, 1, 5));
            }
        }
    }
}