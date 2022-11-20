using System;
using CupheadTAS.Components;
using UnityEngine;

namespace CupheadTAS.Utils; 

internal static class VectorExtensions {
    public static string ToSimpleString(this Vector2 vector2, int decimals = 2) {
        return $"{vector2.x.ToFormattedString(decimals)}, {vector2.y.ToFormattedString(decimals)}";
    }

    public static string ToSimpleString(this Vector3 vector3, int decimals = 2) {
        return $"{vector3.x.ToFormattedString(decimals)}, {vector3.y.ToFormattedString(decimals)}";
    }
}

internal static class NumberExtensions {
    public static string ToFormattedString(this float value, int decimals = 2) {
        return ((double) value).ToFormattedString(decimals);
    }

    public static string ToFormattedString(this double value, int decimals = 2) {
        return value.ToString($"F{decimals}");
    }
    
    public static int ToCeilingFrames(this float seconds) {
        return (int) Math.Ceiling(seconds * CupheadGame.FixedFrameRate);
    }
    
    public static int ToFloorFrames(this float seconds) {
        return (int) Math.Floor(seconds * CupheadGame.FixedFrameRate);
    }
}