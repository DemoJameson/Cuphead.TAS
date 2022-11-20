using System.Collections.Generic;
using CupheadTAS.Utils;
using TAS.Core.Input.Commands;
using TAS.Core.Utils;

namespace CupheadTAS.Commands; 

public class CharmCommand {
    private static readonly Dictionary<string, Charm> Charms = new() {
        {"NONE", Charm.None},
        {"HEART", Charm.charm_health_up_1},
        {"COFFEE", Charm.charm_super_builder},
        {"SMOKE", Charm.charm_smoke_dash},
        {"BOMB", Charm.charm_smoke_dash},
        {"SMOKEBOMB", Charm.charm_smoke_dash},
        {"SMOKE BOMB", Charm.charm_smoke_dash},
        {"P.SUGAR", Charm.charm_parry_plus},
        {"SUGAR", Charm.charm_parry_plus},
        {"TWINHEART", Charm.charm_health_up_2},
        {"TWIN HEART", Charm.charm_health_up_2},
        {"TWIN", Charm.charm_health_up_2},
        {"WHETSTONE", Charm.charm_parry_attack},
        
#if v134
        {"ASTRAL", Charm.charm_chalice},
        {"COOKIE", Charm.charm_chalice},
        {"ASTRAL COOKIE", Charm.charm_chalice},
        {"ASTRALCOOKIE", Charm.charm_chalice},
        {"RELIC", Charm.charm_curse},
        {"BROKEN RELIC", Charm.charm_curse},
        {"BROKENRELIC", Charm.charm_curse},
        {"CURSED RELIC", Charm.charm_curse},
        {"CURSEDRELIC", Charm.charm_curse},
        {"DEVINE RELIC", Charm.charm_curse},
        {"DEVINERELIC", Charm.charm_curse},
        {"HEART RING", Charm.charm_healer},
        {"HEARTRING", Charm.charm_healer},
#endif
    };
    
    static CharmCommand() {
        for (int i = 0; i < MapEquipUICardBackSelect.CHARMS.Length; i++) {
            Charms[(i + 1).ToString()] = MapEquipUICardBackSelect.CHARMS[i];
        }
    }

    [TasCommand("Charm", LegalInMainGame = false)]
    private static void SetCharm(string[] args) {
        if (args.Length == 0) {
            return;
        }

        if (PlayerData.Data.Loadouts.GetPlayerLoadout(PlayerId.PlayerOne) is not {} loadOut) {
            return;
        }

        string charmStr = args[0]?.ToUpper() ?? "";

        if (EnumHelpers<Charm>.TryParse(charmStr, out Charm charm, true) || Charms.TryGetValue(charmStr, out charm)) {
            loadOut.charm = charm;
        }

    }
}