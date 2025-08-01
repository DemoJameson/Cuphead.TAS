﻿using System;
using CupheadTAS.Utils;
using HarmonyLib;
using Rewired;
using TAS;
using TAS.Core.Input;
using TAS.Core.Utils;
using TAS.Shared;

namespace CupheadTAS.Components;

[HarmonyPatch(typeof(Player))]
public class TasRewiredPlayer : PluginComponent {
    private static int currentFrame;
    private static InputFrame previousInput;
    private static InputFrame currentInput;

    private static bool TryGetActions(int actionId, out Actions actions) {
        CupheadButton button = (CupheadButton) actionId;
        actions = button switch {
            CupheadButton.MenuLeft => Actions.Left,
            CupheadButton.MenuRight => Actions.Right,
            CupheadButton.MenuUp => Actions.Up,
            CupheadButton.MenuDown => Actions.Down,
            CupheadButton.Jump => Actions.Jump,
            CupheadButton.Shoot => Actions.Shoot,
            CupheadButton.Super => Actions.ExShoot,
            CupheadButton.SwitchWeapon => Actions.SwitchWeapon,
            CupheadButton.Lock => Actions.Lock,
            CupheadButton.Dash => Actions.Dash,
            CupheadButton.EquipMenu => Actions.Equip,
            CupheadButton.Accept => Actions.Confirm,
            CupheadButton.Cancel => Actions.Back,
            CupheadButton.Pause => Actions.Pause,
            _ => Actions.None
        };

        return actions != Actions.None;
    }

    public static void SetInputs() {
        InputController controller = Manager.Controller;
        currentFrame = controller.CurrentFrameInTas;
        previousInput = controller.Previous;
        currentInput = controller.Current;
    }

    [EnableRun]
    [DisableRun]
    private static void ResetButtonStates() {
        currentFrame = -1;
        previousInput = null;
        currentInput = null;
    }

    [HarmonyPatch(nameof(Player.GetAxis), typeof(string))]
    [HarmonyPrefix]
    private static bool GetAxis(string actionName, ref float __result, Player __instance) {
        if (!Manager.Running || currentInput == null) {
            return true;
        }

        if (EnumHelpers<CupheadButton>.TryParse(actionName, out CupheadButton button)) {
            GetAxis(button, ref __result, __instance);
        }

        return false;
    }

    [HarmonyPatch(nameof(Player.GetAxis), typeof(int))]
    [HarmonyPrefix]
    private static bool GetAxis(int actionId, ref float __result, Player __instance) {
        if (!Manager.Running || currentInput == null) {
            return true;
        }

        CupheadButton button = (CupheadButton) actionId;
        GetAxis(button, ref __result, __instance);
        return false;
    }

    private static void GetAxis(CupheadButton button, ref float __result, Player __instance) {
        if (IsNotPlayerOne(__instance)) {
            __result = 0f;
            return;
        }

        if (button is CupheadButton.MoveHorizontal) {
            if (currentInput.HasActions(Actions.Left)) {
                __result = -1f;
            } else if (currentInput.HasActions(Actions.Right)) {
                __result = 1f;
            }
        } else if (button is CupheadButton.MoveVertical) {
            if (currentInput.HasActions(Actions.Down)) {
                __result = -1f;
            } else if (currentInput.HasActions(Actions.Up)) {
                __result = 1f;
            }
        }
    }

    [HarmonyPatch(nameof(Player.GetButton), typeof(int))]
    [HarmonyPrefix]
    private static bool GetButton(int actionId, ref bool __result, Player __instance) {
        if (!Manager.Running) {
            return true;
        }
        
        if (IsNotPlayerOne(__instance)) {
            __result = false;
            return false;
        }

        if (TryGetActions(actionId, out var actions)) {
            __result = currentInput?.HasActions(actions) == true;
        }

        return false;
    }

    [HarmonyPatch(nameof(Player.GetButtonDown), typeof(int))]
    [HarmonyPrefix]
    private static bool GetButtonDown(int actionId, ref bool __result, Player __instance) {
        if (!Manager.Running) {
            return true;
        }

        if (IsNotPlayerOne(__instance)) {
            __result = false;
            return false;
        }

        if (TryGetActions(actionId, out var actions)) {
            __result = previousInput?.HasActions(actions) != true && currentInput?.HasActions(actions) == true;
        }

        return false;
    }

    [HarmonyPatch(nameof(Player.GetButtonUp), typeof(int))]
    [HarmonyPrefix]
    private static bool GetButtonUp(int actionId, ref bool __result, Player __instance) {
        if (!Manager.Running) {
            return true;
        }

        if (IsNotPlayerOne(__instance)) {
            __result = false;
            return false;
        }

        if (TryGetActions(actionId, out var actions)) {
            __result = previousInput?.HasActions(actions) == true && currentInput?.HasActions(actions) == false;
        }

        return false;
    }

    [HarmonyPatch(nameof(Player.GetButtonTimePressed), typeof(int))]
    [HarmonyPrefix]
    private static bool GetButtonTimePressed(int actionId, ref float __result, Player __instance) {
        if (!Manager.Running) {
            return true;
        }

        if (IsNotPlayerOne(__instance)) {
            __result = 0f;
            return false;
        }

        if (TryGetActions(actionId, out var actions) && currentInput?.HasActions(actions) == true) {
            InputController controller = Manager.Controller;
            for (int i = currentFrame - 1; i >= 0; i--) {
                InputFrame previous = controller.Inputs.GetValueOrDefault(i);
                if (previous == null || !previous.HasActions(actions)) {
                    break;
                }

                __result += 1f / CupheadGame.FixedFrameRate;
            }
        }

        return false;
    }

    [HarmonyPatch(nameof(Player.GetAnyButton))]
    [HarmonyPrefix]
    private static bool GetAnyButton(Player __instance, ref bool __result) {
        if (!Manager.Running) {
            return true;
        }

        __result = false;

        CupheadButton[] buttons = (CupheadButton[]) Enum.GetValues(typeof(CupheadButton));
        foreach (CupheadButton button in buttons) {
            if (__instance.GetButton((int) button)) {
                __result = true;
                return false;
            }
        }

        return false;
    }

    [HarmonyPatch(nameof(Player.GetAnyButtonDown))]
    [HarmonyPrefix]
    private static bool GetAnyButtonDown(Player __instance, ref bool __result) {
        if (!Manager.Running) {
            return true;
        }

        __result = false;

        CupheadButton[] buttons = (CupheadButton[]) Enum.GetValues(typeof(CupheadButton));
        foreach (CupheadButton button in buttons) {
            if (__instance.GetButtonDown((int) button)) {
                __result = true;
                return false;
            }
        }

        return false;
    }

    [HarmonyPatch(nameof(Player.GetAnyButtonUp))]
    [HarmonyPrefix]
    private static bool GetAnyButtonUp(Player __instance, ref bool __result) {
        if (!Manager.Running) {
            return true;
        }

        __result = false;

        CupheadButton[] buttons = (CupheadButton[]) Enum.GetValues(typeof(CupheadButton));
        foreach (CupheadButton button in buttons) {
            if (__instance.GetButtonUp((int) button)) {
                __result = true;
                return false;
            }
        }

        return false;
    }

    [HarmonyPatch(typeof(CupheadInput.AnyPlayerInput), nameof(CupheadInput.AnyPlayerInput.GetAnyButtonDown))]
    [HarmonyPrefix]
    private static bool AnyPlayerInputGetAnyButtonDown(CupheadInput.AnyPlayerInput __instance, ref bool __result) {
        if (!Manager.Running) {
            return true;
        }

        CupheadButton[] buttons = (CupheadButton[]) Enum.GetValues(typeof(CupheadButton));
        foreach (CupheadButton button in buttons) {
            if (__instance.GetButtonDown(button)) {
                __result = true;
                return false;
            }
        }

        return false;
    }
    
    [HarmonyPatch(typeof(CupheadInput.AnyPlayerInput), nameof(CupheadInput.AnyPlayerInput.GetAnyButtonHeld))]
    [HarmonyPrefix]
    private static bool AnyPlayerInputGetAnyButtonHold(CupheadInput.AnyPlayerInput __instance, ref bool __result) {
        if (!Manager.Running) {
            return true;
        }

        __result = false;

        CupheadButton[] buttons = (CupheadButton[]) Enum.GetValues(typeof(CupheadButton));
        foreach (CupheadButton button in buttons) {
            if (__instance.GetButton(button)) {
                __result = true;
                return false;
            }
        }

        return false;
    }

    [HarmonyPatch(typeof(InputManager_Base), "Update")]
    [HarmonyPrefix]
    private static bool InputManagerUpdate() {
        return !Manager.Running;
    }

    [HarmonyPatch(typeof(InputManager_Base), "FixedUpdate")]
    [HarmonyPrefix]
    private static bool InputManagerFixedUpdate() {
        return !Manager.Running;
    }
    
    private static bool IsNotPlayerOne(Player player) => player != PlayerManager.GetPlayerInput(PlayerId.PlayerOne);
}