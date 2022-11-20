using System.Collections.Generic;
using CupheadTAS.Utils;
using TAS.Core.Input.Commands;

namespace CupheadTAS.Commands;

public class SuperCommand {
    private static readonly Dictionary<string, Super> Supers = new() {
        {"NONE", Super.None},
        {"I", Super.level_super_beam},
        {"ENERGY", Super.level_super_beam},
        {"BEAM", Super.level_super_beam},
        {"ENERGYBEAM", Super.level_super_beam},
        {"ENERGY BEAM", Super.level_super_beam},
        {"II", Super.level_super_invincible},
        {"INVINCIBLE", Super.level_super_invincible},
        {"INVINCIBILITY", Super.level_super_invincible},
        {"III", Super.level_super_ghost},
        {"GIANT", Super.level_super_ghost},
        {"GHOST", Super.level_super_ghost},
        {"GIANTGHOST", Super.level_super_ghost},
        {"GIANT GHOST", Super.level_super_ghost},
    };

    static SuperCommand() {
        for (int i = 0; i < MapEquipUICardBackSelect.SUPERS.Length; i++) {
            Supers[(i + 1).ToString()] = MapEquipUICardBackSelect.SUPERS[i];
        }
    }

    [TasCommand("Super", LegalInMainGame = false)]
    private static void SetSuper(string[] args) {
        if (args.Length == 0) {
            return;
        }

        if (PlayerData.Data.Loadouts.GetPlayerLoadout(PlayerId.PlayerOne) is not { } loadOut) {
            return;
        }

        string superStr = args[0]?.ToUpper() ?? "";

        if (EnumHelpers<Super>.TryParse(superStr, out Super super, true) || Supers.TryGetValue(superStr, out super)) {
            loadOut.super = super;
        }
    }
}