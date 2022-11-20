using System;
using System.IO;
using BepInEx.Bootstrap;
using TAS;
using UnityEngine;

namespace CupheadTAS.Components; 

public class DisableRunWhenReload : PluginComponent {
    private string scriptsPath;
    private FileSystemWatcher watcher;

    private void Awake() {
        scriptsPath = Path.Combine(Path.Combine(Directory.GetParent(Application.dataPath).ToString(), "BepInEx"), "scripts");
        if (File.Exists(Path.Combine(scriptsPath, "Cuphead.TAS.dll")) && Chainloader.PluginInfos.ContainsKey("com.bepis.bepinex.scriptengine")) {
            WatchScriptsFolder();
        }
    }

    private void WatchScriptsFolder() {
        try {
            watcher = new FileSystemWatcher {
                Path = scriptsPath,
                NotifyFilter = NotifyFilters.LastWrite,
            };

            watcher.Changed += (s, e) => {
                if (Path.GetFileName(e.Name) == "Cuphead.TAS.dll" && Manager.Running) {
                    Manager.DisableRun();
                }
            };

            watcher.EnableRaisingEvents = true;
        } catch (Exception) {
            Logger.LogWarning($"Failed watching folder: {Path.GetDirectoryName(scriptsPath)})");
            OnDestroy();
        }
    }

    private void OnDestroy() {
        watcher?.Dispose();
        watcher = null;
    }
}