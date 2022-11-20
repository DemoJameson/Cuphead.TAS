using System.Collections.Generic;
using BepInEx.Configuration;
using HarmonyLib;
using TAS;
using UnityEngine;

namespace CupheadTAS.Components;

[HarmonyPatch]
public class SkipRenderDuringFastForward : PluginComponent {
    private static ConfigEntry<bool> skipRenderDuringFastForward; 
    
    private static readonly List<Renderer> sceneRenderers = new();
    private static readonly List<Renderer> renderers = new();

    [DisableRun]
    private static void ResetRenderers() {
        foreach (Renderer renderer in sceneRenderers) {
            if (renderer) {
                renderer.enabled = true;
            }
        }
            
        foreach (Renderer renderer in renderers) {
            if (renderer) {
                renderer.enabled = true;
            }
        }

        sceneRenderers.Clear();
        renderers.Clear();
    }

    [HarmonyPatch(typeof(AbstractMonoBehaviour), "Awake")]
    [HarmonyPrefix]
    private static void AddController(AbstractMonoBehaviour __instance) {
        if (Manager.UltraFastForwarding && skipRenderDuringFastForward.Value) {
            foreach (Renderer renderer in __instance.GetComponentsInChildren<Renderer>()) {
                if (renderer.enabled) {
                    renderer.enabled = false;
                    renderers.Add(renderer);
                }
            }
        }
    }

    private void Awake() {
        skipRenderDuringFastForward = Plugin.Instance.Config.Bind("FastForward", "Skip Render During Fast Forward (Might Desync)", false, isAdvanced: true);

        HookHelper.ActiveSceneChanged((_, scene) => {
            if (!Manager.UltraFastForwarding || !skipRenderDuringFastForward.Value) {
                return;
            }
        
            sceneRenderers.Clear();
            foreach (GameObject go in scene.GetRootGameObjects()) {
                foreach (Renderer renderer in go.GetComponentsInChildren<Renderer>()) {
                    if (renderer.enabled) {
                        renderer.enabled = false;
                        sceneRenderers.Add(renderer);
                    }
                }
            }
        });
    }

    private void Update() {
        if (!skipRenderDuringFastForward.Value) {
            return;
        }
        
        if (Manager.Running && !Manager.UltraFastForwarding) {
            ResetRenderers();
        }
    }
}