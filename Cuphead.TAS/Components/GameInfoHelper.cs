using System;
using System.Collections.Generic;
using CupheadTAS.Utils;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using TAS.Core.Utils;
using UnityEngine;
using static PlayerData.PlayerLoadouts;

namespace CupheadTAS.Components;

[HarmonyPatch]
public class GameInfoHelper : PluginComponent {
    private static Vector3? lastPlayerPosition;
    private static string lastLevelName;
    private static string lastTime;
    private static string lastInfo;
    private static int lastEventIndex;
    private static float overDamage;
    private static readonly Dictionary<Weapon, int> weaponsTimer = new();
    private static readonly Dictionary<Weapon, int> exWeaponsTimer = new();
    private static int chargeFrames;
    private static int exFireFrames;
    private static int exDelayFrames;
    private static int exPlaneFireFrames;
    private static int exPlaneDelayFrames;
    private static readonly List<string> infos = new();
    private static readonly List<string> statuses = new();

    [HarmonyPatch(typeof(AbstractLevelWeapon), nameof(AbstractLevelWeapon.fireWeapon_cr), MethodType.Enumerator)]
    [HarmonyPatch(typeof(AbstractLevelWeapon), nameof(AbstractLevelWeapon.chargeFireWeapon_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void AbstractLevelWeaponFireWeaponCr(ILContext ilContext) {
        ILCursor ilCursor = new(ilContext);

        ilCursor.GotoNext(i => i.OpCode == OpCodes.Ldfld && i.Operand.ToString().EndsWith("::$this"));
        object thisOperand = ilCursor.Next.Operand;
        ilCursor.Goto(0);

        ilCursor.GotoNext(i => i.OpCode == OpCodes.Ldfld && i.Operand.ToString().EndsWith("::mode"));
        object modeOperand = ilCursor.Next.Operand;
        ilCursor.Goto(0);

        while (ilCursor.TryGotoNext(i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("::<t>"))) {
            ilCursor.Emit(OpCodes.Ldarg_0)
                .Emit(OpCodes.Ldfld, thisOperand)
                .Emit(OpCodes.Ldarg_0)
                .Emit(OpCodes.Ldfld, modeOperand)
                .EmitDelegate<Func<float, AbstractLevelWeapon, AbstractLevelWeapon.Mode, float>>((t, levelWeapon, mode) => {
                    int frames = (levelWeapon.rapidFireRate - t).ToCeilingFrames();
                    if (mode == AbstractLevelWeapon.Mode.Basic) {
                        weaponsTimer[levelWeapon.id] = frames;
                    } else {
                        exWeaponsTimer[levelWeapon.id] = frames;
                    }

                    return t;
                });
            ilCursor.Index++;
        }
    }

    [HarmonyPatch(typeof(AbstractPlaneWeapon), nameof(AbstractPlaneWeapon.fireWeapon_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void AbstractPlaneWeaponFireWeaponCr(ILContext ilContext) {
        ILCursor ilCursor = new(ilContext);

        ilCursor.GotoNext(i => i.OpCode == OpCodes.Ldfld && i.Operand.ToString().EndsWith("::$this"));
        object thisOperand = ilCursor.Next.Operand;

        ilCursor.Goto(0);
        while (ilCursor.TryGotoNext(i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("::<t>"))) {
            ilCursor.Emit(OpCodes.Ldarg_0)
                .Emit(OpCodes.Ldfld, thisOperand)
                .EmitDelegate<Func<float, AbstractPlaneWeapon, float>>((t, planeWeapon) => {
                    Weapon weapon = planeWeapon.index switch {
                        0 => Weapon.plane_weapon_peashot,
                        1 => Weapon.plane_weapon_laser,
                        2 => Weapon.plane_weapon_bomb,
#if v134
                        3 => Weapon.plane_chalice_weapon_3way,
                        4 => Weapon.plane_chalice_weapon_bomb,
#endif
                        _ => Weapon.None
                    };
                    weaponsTimer[weapon] = (planeWeapon.rapidFireRate - t).ToCeilingFrames();
                    return t;
                });
            ilCursor.Index++;
        }
    }

    [HarmonyPatch(typeof(WeaponCharge), nameof(WeaponCharge.FixedUpdate))]
    [HarmonyPostfix]
    private static void WeaponChargeFixedUpdate(WeaponCharge __instance) {
        chargeFrames =
            (WeaponProperties.LevelWeaponCharge.Basic.timeStateThree - __instance.timeCharged).ToFloorFrames();
        if (chargeFrames < 0) {
            chargeFrames = 0;
        }
    }

    [HarmonyPatch(typeof(LevelPlayerWeaponManager), nameof(LevelPlayerWeaponManager.StartEx))]
    [HarmonyPostfix]
    private static void LevelPlayerWeaponManagerStartEx() {
        exDelayFrames = 34;
        exFireFrames = 16;
    }

    [HarmonyPatch(typeof(PlanePlayerWeaponManager), nameof(PlanePlayerWeaponManager.StartEx))]
    [HarmonyPostfix]
    private static void PlanePlayerWeaponManagerStartEx() {
        exPlaneDelayFrames = 54;
        exPlaneFireFrames = 31;
    }

    private void Awake() {
        HookHelper.ActiveSceneChanged(() => {
            lastPlayerPosition = null;
            lastLevelName = null;
            lastTime = null;
            lastInfo = null;
            lastEventIndex = 0;
            overDamage = 0;
            weaponsTimer.Clear();
            exWeaponsTimer.Clear();
            chargeFrames = 0;
            exDelayFrames = 0;
            exFireFrames = 0;
            exPlaneDelayFrames = 0;
            exPlaneFireFrames = 0;
        });
    }

    private void Update() {
        if (exDelayFrames > 0) {
            exDelayFrames--;
        }

        if (exFireFrames > 0) {
            exFireFrames--;
        }

        if (exPlaneDelayFrames > 0) {
            exPlaneDelayFrames--;
        }

        if (exPlaneFireFrames > 0) {
            exPlaneFireFrames--;
        }
    }

    public static string Info {
        get {
            CupheadGame game = CupheadGame.Instance;

            if (lastLevelName == CurrentSceneName && lastTime == game.CurrentTime || PlayerManager.players == null) {
                lastLevelName = CurrentSceneName;
                return lastInfo;
            }

            if (PlayerManager.Current is LevelPlayerController levelPlayer) {
                LevelPlayerMotor motor = levelPlayer.motor;
                infos.Clear();
                statuses.Clear();

                Vector3 position = levelPlayer.transform.position;
                infos.Add($"Pos:   {position.ToSimpleString()}");
                infos.Add($"Speed: {(motor.velocityManager.Total * CupheadTime.FixedDelta).ToSimpleString()}");

                lastPlayerPosition ??= position;
                infos.Add($"Vel:   {(position - lastPlayerPosition.Value).ToSimpleString()}");
                lastPlayerPosition = position;

                if (Level.Current is {type: Level.Type.Battle, timeline: {health: > 0f} timeline}) {
                    float totalHealth = timeline.health;
                    float currentHealth = totalHealth - timeline.damage - overDamage;
                    string boss = $"Boss:  {currentHealth:F2} {overDamage:F2}";

                    List<Level.Timeline.Event> events = timeline.events;

                    for (int index = lastEventIndex; index < events.Count; index++) {
                        Level.Timeline.Event @event = events[index];
                        float nextPhaseHealth = totalHealth * @event.percentage;
                        if (currentHealth > nextPhaseHealth) {
                            if (lastEventIndex != index) {
                                overDamage = currentHealth - totalHealth * events[lastEventIndex].percentage;
                                Debug.LogWarning(lastEventIndex + ": " + overDamage);
                            }

                            lastEventIndex = index;
                            break;
                        } else if (index == events.Count - 1) {
                            lastEventIndex = events.Count;
                            overDamage = currentHealth - nextPhaseHealth;
                        }
                    }

                    infos.Add(boss);
                }

                infos.Add($"Super: {levelPlayer.stats.SuperMeter:F2}");

                PlayerLoadout loadOut = PlayerData.Data.Loadouts.GetPlayerLoadout(PlayerId.PlayerOne);
                LevelPlayerWeaponManager weaponManager = levelPlayer.weaponManager;

                string weaponInfo = $"EX: {exDelayFrames} {exFireFrames}  ";
                if (weaponsTimer.TryGetValue(loadOut.primaryWeapon, out int frame1)) {
                    weaponInfo += $"{GetWeaponName(loadOut.primaryWeapon)}: {frame1}";
                }

                if (weaponsTimer.TryGetValue(loadOut.secondaryWeapon, out int frame2)) {
                    if (weaponManager.currentWeapon == loadOut.secondaryWeapon) {
                        weaponInfo += " -> ";
                    } else {
                        weaponInfo += " <- ";
                    }

                    weaponInfo += $"{GetWeaponName(loadOut.secondaryWeapon)}: {frame2}";

                    if (loadOut.secondaryWeapon == Weapon.level_weapon_charge) {
                        weaponInfo += $" {chargeFrames}";
                    }
                }

                if (weaponInfo.IsNotEmpty()) {
                    infos.Add(weaponInfo);
                }

                if (Level.Current is {Started: false}) {
                    statuses.Add($"NoControl");
                } else {
                    if (!motor.allowInput) {
                        statuses.Add("NoInput");
                    }

                    if (motor.Grounded) {
                        statuses.Add("Ground");
                    }

                    if (motor.dashManager.state == LevelPlayerMotor.DashManager.State.Ready &&
                        (!motor.Grounded || motor.dashManager.timeSinceGroundDash > 0.1f)) {
                        statuses.Add("Dash");
                    }

                    if (!levelPlayer.CanTakeDamage) {
                        statuses.Add("Invincible");
                    }
                }

                infos.Add(statuses.Join(delimiter: " "));
                infos.Add($"{game.CurrentTime}");
                infos.Add($"[{game.LevelName}]");

                lastTime = game.CurrentTime;
                return lastInfo = infos.Join(delimiter: "\n");
            } else if (PlayerManager.Current is PlanePlayerController planePlayer) {
                PlanePlayerMotor motor = planePlayer.motor;
                infos.Clear();
                statuses.Clear();

                Vector3 position = planePlayer.transform.position;
                infos.Add($"Pos:   {position.ToSimpleString()}");
                infos.Add($"Speed: {(motor.Velocity * CupheadTime.FixedDelta).ToSimpleString()}");

                lastPlayerPosition ??= position;
                infos.Add($"Vel:   {(position - lastPlayerPosition.Value).ToSimpleString()}");
                lastPlayerPosition = position;

                if (Level.Current is {type: Level.Type.Battle, timeline: {health: > 0f} timeline}) {
                    float totalHealth = timeline.health;
                    float currentHealth = totalHealth - timeline.damage;
                    string boss = $"Boss:  {totalHealth - timeline.damage:F2}";

                    List<Level.Timeline.Event> events = timeline.events;
                    for (int index = lastEventIndex; index < events.Count; index++) {
                        Level.Timeline.Event @event = events[index];
                        float nextPhaseHealth = totalHealth * @event.percentage + overDamage;
                        if (currentHealth > nextPhaseHealth) {
                            if (lastEventIndex != index) {
                                overDamage = currentHealth - totalHealth * events[lastEventIndex].percentage;
                                nextPhaseHealth = totalHealth * @event.percentage + overDamage;
                            }

                            boss += $"  Next: {nextPhaseHealth:F2}";
                            lastEventIndex = index;
                            break;
                        }
                    }

                    infos.Add(boss);
                }

                infos.Add($"Super: {planePlayer.stats.SuperMeter:F2}");

                string weaponInfo = $"EX: {exPlaneDelayFrames} {exPlaneFireFrames}  ";

                Weapon weapon1 = Weapon.plane_weapon_peashot;
                Weapon weapon2 = Weapon.plane_weapon_bomb;
                Weapon currentWeapon = planePlayer.weaponManager.currentWeapon;
                if (planePlayer.Shrunk) {
                    currentWeapon = Weapon.plane_weapon_peashot;
                }

#if v134
                if (planePlayer.stats.isChalice) {
                    weapon1 = Weapon.plane_chalice_weapon_3way;
                    weapon2 = Weapon.plane_chalice_weapon_bomb;
                }
#endif

                if (weaponsTimer.TryGetValue(weapon1, out int frame1)) {
                    weaponInfo += $"{GetWeaponName(weapon1)}: {frame1}";
                }

                if (weaponsTimer.TryGetValue(weapon2, out int frame2)) {
                    if (currentWeapon == weapon2) {
                        weaponInfo += " -> ";
                    } else {
                        weaponInfo += " <- ";
                    }

                    weaponInfo += $"{GetWeaponName(weapon2)}: {frame2}";
                }

                if (weaponInfo.IsNotEmpty()) {
                    infos.Add(weaponInfo);
                }

                if (Level.Current is {Started: false}) {
                    statuses.Add("NoControl");
                } else {
                    if (!planePlayer.CanTakeDamage) {
                        statuses.Add("Invincible");
                    }

                    PlanePlayerParryController parryController = planePlayer.parryController;
                    if (!planePlayer.Shrunk && !planePlayer.WeaponBusy && !(planePlayer.stats.StoneTime > 0f) &&
                        parryController.state == PlanePlayerParryController.ParryState.Ready) {
                        statuses.Add("Parry");
                    } else if (parryController.state == PlanePlayerParryController.ParryState.Cooldown) {
                        statuses.Add($"NoParry({(0.3f - parryController.timeSinceParry).ToCeilingFrames()})");
                    }
                }

                infos.Add(statuses.Join(delimiter: " "));
                infos.Add($"{game.CurrentTime}");
                infos.Add($"[{game.LevelName}]");

                lastTime = game.CurrentTime;
                return lastInfo = infos.Join(delimiter: "\n");
            } else if (Map.Current is { } map) {
                MapPlayerController player = map.players[0];

                statuses.Clear();
                infos.Clear();

                string position = $"Pos:   {player.transform.position.ToSimpleString()}";
                string speed = $"Speed: {(player.motor.velocity * CupheadTime.FixedDelta).ToSimpleString()}";
                lastPlayerPosition ??= player.transform.position;
                string velocity = $"Vel:   {(player.transform.position - lastPlayerPosition.Value).ToSimpleString()}";
                lastPlayerPosition = player.transform.position;

                infos.Add(position);
                infos.Add(speed);
                infos.Add(velocity);

                infos.Add($"[{game.LevelName}]");
                lastTime = "";
                return infos.Join(delimiter: "\n");
            } else {
                return $"[{game.LevelName}]";
            }
        }
    }

    private static string GetWeaponName(Weapon weapon) {
        return weapon switch {
            Weapon.level_weapon_peashot => "Peashooter",
            Weapon.level_weapon_spreadshot => "Spread",
            Weapon.level_weapon_homing => "Chaser",
            Weapon.level_weapon_bouncer => "Lobber",
            Weapon.level_weapon_charge => "Charge",
            Weapon.level_weapon_boomerang => "Roundabout",
            Weapon.plane_weapon_peashot => "Peashooter",
            Weapon.plane_weapon_bomb => "Bomb",
#if v134
            Weapon.level_weapon_crackshot => "Crackshot",
            Weapon.level_weapon_wide_shot => "Converge",
            Weapon.level_weapon_upshot => "TwistUp",
            Weapon.plane_chalice_weapon_3way => "Peashooter",
            Weapon.plane_chalice_weapon_bomb => "Bomb",
#endif
            _ => weapon.ToString().Replace("level_weapon_", ""),
        };
    }

    public static string FormatTime(float time) {
        TimeSpan timeSpan = TimeSpan.FromSeconds(time);
        string formatted =
            $"{timeSpan.Minutes.ToString().PadLeft(2, '0')}:{timeSpan.Seconds.ToString().PadLeft(2, '0')}.{timeSpan.Milliseconds.ToString().PadLeft(3, '0')}";
        return $"{formatted}({time.ToCeilingFrames()})";
    }
}