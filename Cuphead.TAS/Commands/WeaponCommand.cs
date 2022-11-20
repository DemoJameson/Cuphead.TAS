using System.Collections.Generic;
using CupheadTAS.Utils;
using TAS.Core.Input.Commands;
using TAS.Core.Utils;

namespace CupheadTAS.Commands;

public class WeaponCommand {
    private static readonly Dictionary<string, Weapon> Weapons = new() {
        {"NONE", Weapon.None},
        {"PEASHOOTER", Weapon.level_weapon_peashot},
        {"SPREAD", Weapon.level_weapon_spreadshot},
        {"CHASER", Weapon.level_weapon_homing},
        {"LOBBER", Weapon.level_weapon_bouncer},
        {"CHARGE", Weapon.level_weapon_charge},
        {"ROUNDABOUT", Weapon.level_weapon_boomerang},

#if v134
        {"CRACKSHOT", Weapon.level_weapon_crackshot},
        {"CONVERGE", Weapon.level_weapon_wide_shot},
        {"TWIST-UP", Weapon.level_weapon_upshot},
        {"TWIST", Weapon.level_weapon_upshot},
#endif
    };

    static WeaponCommand() {
        for (int i = 0; i < MapEquipUICardBackSelect.WEAPONS.Length; i++) {
            Weapons[(i + 1).ToString()] = MapEquipUICardBackSelect.WEAPONS[i];
        }
    }

    [TasCommand("Weapon", LegalInMainGame = false)]
    private static void SetWeapons(string[] args) {
        if (args.Length == 0) {
            return;
        }

        if (PlayerData.Data.Loadouts.GetPlayerLoadout(PlayerId.PlayerOne) is not { } loadOut) {
            return;
        }

        string weapon1Str = args[0]?.ToUpper() ?? "";
        string weapon2Str = args.GetValueOrDefault(1)?.ToUpper() ?? "";

        if (EnumHelpers<Weapon>.TryParse(weapon1Str, out Weapon weapon1, true) || Weapons.TryGetValue(weapon1Str, out weapon1)) {
            if (weapon1 != Weapon.None) {
                loadOut.primaryWeapon = weapon1;
            }
        }

        if (EnumHelpers<Weapon>.TryParse(weapon2Str, out Weapon weapon2, true) || Weapons.TryGetValue(weapon2Str, out weapon2)) {
            loadOut.secondaryWeapon = weapon2;
        }
    }
}