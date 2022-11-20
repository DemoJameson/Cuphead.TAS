using System;
using CupheadTAS.Utils;
using HarmonyLib;
using MonoMod.Cil;
using TAS;
using TAS.Core;
using TAS.Core.Input;
using UnityEngine;

namespace CupheadTAS.Components;

[HarmonyPatch]
public class CupheadGame : PluginComponent, IGame {
    public static CupheadGame Instance { get; private set; }
    public static int FixedFrameRate => 60;
    public static bool LoadingScene;
    public string CurrentTime => GameInfoHelper.FormatTime(Level.Current?.LevelTime ?? 0f);
    public float FastForwardSpeed => 10;
    public float SlowForwardSpeed => 0.1f;
    public string LevelName => CurrentSceneName;
    public ulong FrameCount => (ulong) Time.frameCount;
    public bool IsLoading => LoadingScene;

    private void Awake() {
        Instance = this;
    }

    public string GameInfo => Components.GameInfoHelper.Info;

    public void SetFrameRate(float multiple) {
        int newFrameRate = (int) (FixedFrameRate * multiple);
        Time.timeScale = Time.timeScale == 0 ? 0 : (float) newFrameRate / FixedFrameRate;
        Time.captureFramerate = newFrameRate;
        Application.targetFrameRate = newFrameRate;
        Time.maximumDeltaTime = Time.fixedDeltaTime;
        QualitySettings.vSyncCount = 0;
    }

    public void SetInputs(InputFrame currentInput) {
        TasRewiredPlayer.SetInputs();
    }

    [HarmonyPatch(typeof(SceneLoader), nameof(SceneLoader.loop_cr), MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void SceneLoaderLoopCr(ILContext ilContext) {
        ILCursor cursor1 = new(ilContext);

        if (cursor1.TryGotoNext(i => i.MatchCall<SceneLoader>(nameof(SceneLoader.load_cr)))) {
            ILCursor cursor2 = cursor1.Clone();
            if (cursor2.TryGotoNext(i => i.MatchCall<SceneLoader>(nameof(SceneLoader.iconFadeOut_cr)))) {
                cursor1.EmitDelegate<Action>(() => {
                    if (Manager.Running) {
                        LoadingScene = true;
                    }
                });

                cursor2.EmitDelegate<Action>(() => LoadingScene = false);
            }
        }
    }

    [EnableRun]
    [DisableRun]
    private static void DisableRun() {
        LoadingScene = false;
    }
}