using System;
using CupheadTAS.Components;
using TAS.Core.Input.Commands;
using TAS.Core.Utils;

namespace CupheadTAS.Commands;

public class SeedCommand : PluginComponent {
    public static string Seed => seed ?? "";
    private static string seed;
    
    [TasCommand("Seed", LegalInMainGame = false)]
    private static void Health(string[] args) {
        if (args.IsEmpty()) {
            return;
        }

        seed = args[0];
    }

    private void Awake() {
        HookHelper.ActiveSceneChanged(() => {
            seed = "";
        });
    }
}