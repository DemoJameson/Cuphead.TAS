using System;
using System.Collections.Generic;
using System.Linq;
using CupheadTAS.Commands;
using CupheadTAS.Utils;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using TAS;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace CupheadTAS.Components;

[HarmonyPatch]
public class FixedRandom : PluginComponent {
    private static bool runOrigRandom;

    [HarmonyPatch(typeof(UIImageAnimationLoop), nameof(UIImageAnimationLoop.Shuffle))]
    [HarmonyILManipulator]
    private static void UIImageAnimationLoopShuffle(ILContext ilContext) {
        ILCursor ilCursor = new(ilContext);
        if (ilCursor.TryGotoNext(i => i.OpCode == OpCodes.Newobj && i.Operand.ToString().EndsWith("Random::.ctor()"))) {
            ilCursor.Index++;
            ilCursor.EmitDelegate<Func<System.Random, System.Random>>(random => new System.Random(SceneManager.GetActiveScene().name.GetHashCode()));
        }
    }

    [HarmonyPatch(typeof(Random), nameof(Random.value), MethodType.Getter)]
    [HarmonyPrefix]
    private static bool RandomValue2(ref float __result) {
        return TryFixedRandom(ref __result, () => Random.value);
    }

    [HarmonyPatch(typeof(Random), nameof(Random.Range), typeof(int), typeof(int))]
    [HarmonyPrefix]
    private static bool RandomRangeInt(int min, int max, ref int __result) {
        return TryFixedRandom(ref __result, () => Random.Range(min, max));
    }
    
    [HarmonyPatch(typeof(Random), nameof(Random.Range), typeof(float), typeof(float))]
    [HarmonyPrefix]
    private static bool RandomRangeFloat(float min, float max, ref float __result) {
        return TryFixedRandom(ref __result, () => Random.Range(min, max));
    }
    
    [HarmonyPatch(typeof(Random), nameof(Random.insideUnitSphere), MethodType.Getter)]
    [HarmonyPrefix]
    private static bool RandomInsideUnitSphere(ref Vector3 __result) {
        return TryFixedRandom(ref __result, () => Random.insideUnitSphere);
    }
    
    [HarmonyPatch(typeof(Random), nameof(Random.onUnitSphere), MethodType.Getter)]
    [HarmonyPrefix]
    private static bool RandomOnUnitSphere(ref Vector3 __result) {
        return TryFixedRandom(ref __result, () => Random.onUnitSphere);
    }
    
    [HarmonyPatch(typeof(Random), nameof(Random.insideUnitCircle), MethodType.Getter)]
    [HarmonyPrefix]
    private static bool RandomInsideUnitCircle(ref Vector2 __result) {
        return TryFixedRandom(ref __result, () => Random.insideUnitCircle);
    }
    
    [HarmonyPatch(typeof(Random), nameof(Random.rotation), MethodType.Getter)]
    [HarmonyPrefix]
    private static bool RandomRotation(ref Quaternion __result) {
        return TryFixedRandom(ref __result, () => Random.rotation);
    }
    
    [HarmonyPatch(typeof(Random), nameof(Random.rotationUniform), MethodType.Getter)]
    [HarmonyPrefix]
    private static bool RandomRotationUniform(ref Quaternion __result) {
        return TryFixedRandom(ref __result, () => Random.rotationUniform);
    }
    
    private static bool TryFixedRandom<T>(ref T result, Func<T> func) {
        if (Manager.Running && !runOrigRandom) {
            runOrigRandom = true;
            FixedRandomState();
            result = func();
            runOrigRandom = false;
            return false;
        } else {
            return true;
        }
    }

    private static void FixedRandomState(params object[] objects) {
        List<object> seeds = new(objects) {SceneLoader.SceneName + SeedCommand.Seed};
        if (!CupheadGame.Instance.IsLoading) {
            if (Level.Current is { } level) {
                seeds.Add(level.LevelTime.ToCeilingFrames());

                if (PlayerManager.players != null) {
                    if (PlayerManager.Current is LevelPlayerController playerController) {
                        seeds.Add(playerController.transform.position);

                        LevelPlayerMotor motor = playerController.motor;
                        seeds.Add(motor.velocityManager.Total);
                        seeds.Add(motor.DashDirection);
                        seeds.Add(motor.dashManager.state);
                        seeds.Add(motor.Ducking);
                        seeds.Add(motor.Parrying);
                        seeds.Add(motor.Locked);
                        seeds.Add(motor.Grounded);
                        seeds.Add(motor.JumpState);
                        seeds.Add(motor.hitManager.state);
                        seeds.Add(motor.superManager.state);

                        LevelPlayerWeaponManager weaponManager = playerController.weaponManager;
                        seeds.Add(weaponManager.currentWeapon);
                        seeds.Add(weaponManager.IsShooting);
                        seeds.Add(weaponManager.basic.firing);
                        seeds.Add(weaponManager.ex.firing);
                    } else if (PlayerManager.Current is PlanePlayerController planePlayer) {
                        seeds.Add(planePlayer.transform.position);
                        seeds.Add(planePlayer.Shrunk);
                        seeds.Add(planePlayer.Parrying);
                        seeds.Add(planePlayer.WeaponBusy);

                        PlanePlayerMotor motor = planePlayer.motor;
                        seeds.Add(motor.Velocity);

                        PlanePlayerWeaponManager weaponManager = planePlayer.weaponManager;
                        seeds.Add(weaponManager.currentWeapon);
                        seeds.Add(weaponManager.IsShooting);
                        seeds.Add(weaponManager.state);
                        seeds.Add(weaponManager.states.ex);
                    }
                }
            }
        }

        Random.InitState(seeds.CombineHashcode());
    }
}

internal static class HashCodeExtension {
    public static int CombineHashcode(this IEnumerable<object> objects) {
        unchecked {
            return objects.Aggregate(17, (current, o) => current * -1521134295 + o.GetHashCode());
        }
    }
}