using System;
using System.Collections.Generic;

namespace TAS.Shared;

[Flags]
public enum Actions {
    None = 0,
    Left = 1 << 0,
    Right = 1 << 1,
    Up = 1 << 2,
    Down = 1 << 3,
    Jump = 1 << 4,
    Shoot = 1 << 5,
    ExShoot = 1 << 6,
    SwitchWeapon = 1 << 7,
    Lock = 1 << 8,
    Dash = 1 << 9,
    Equip = 1 << 10,
    Confirm = 1 << 11,
    Back = 1 << 12,
    Pause = 1 << 13,
}

public static class ActionsUtils {
    public static readonly Dictionary<char, Actions> Chars = new() {
        {'L', Actions.Left},
        {'R', Actions.Right},
        {'U', Actions.Up},
        {'D', Actions.Down},
        {'J', Actions.Jump},
        {'S', Actions.Shoot},
        {'E', Actions.ExShoot},
        {'W', Actions.SwitchWeapon},
        {'O', Actions.Lock},
        {'X', Actions.Dash},
        {'Q', Actions.Equip},
        {'C', Actions.Confirm},
        {'B', Actions.Back},
        {'P', Actions.Pause},
    };
}