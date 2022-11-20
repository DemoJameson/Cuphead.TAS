using System;
using HarmonyLib;
using MonoMod.Cil;
using UnityEngine;

namespace CupheadTAS.Components; 

[HarmonyPatch]
public class CutLog : PluginComponent {
    [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.Save))]
    [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.SaveAll))]
    [HarmonyILManipulator]
    private static void PlayerDataSave(ILContext ilContext) {
        ILCursor ilCursor = new(ilContext);
        if (ilCursor.TryGotoNext(i => i.MatchCall<Debug>("Log"))) {
            ilCursor.EmitDelegate<Func<string, string>>(_ => "[PlayerData] Data saving");
        }
    }
}