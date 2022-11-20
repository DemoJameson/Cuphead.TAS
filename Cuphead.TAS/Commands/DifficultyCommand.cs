using System.Collections.Generic;
using TAS.Core.Input.Commands;
using UnityEngine;

namespace CupheadTAS.Commands; 

public class DifficultyCommand {
    private static readonly Dictionary<string, Level.Mode> Difficulties = new() {
        {"EASY", Level.Mode.Easy},
        {"NORMAL", Level.Mode.Normal},
        {"HARD", Level.Mode.Hard},
        {"SIMPLE", Level.Mode.Easy},
        {"REGULAR", Level.Mode.Normal},
        {"EXPERT", Level.Mode.Hard},
    };

    [TasCommand("Difficulty", LegalInMainGame = false)]
    private static void Difficulty(string[] args) {
        if (args.Length == 0) {
            return;
        }
        
        string difficulty = args[0].ToUpper();

        if (Difficulties.TryGetValue(difficulty, out Level.Mode mode)) {
            Level.CurrentMode = mode;
        }
    }
}