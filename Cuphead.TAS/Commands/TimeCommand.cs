using CupheadTAS.Components;
using TAS.Core.Input.Commands;

namespace CupheadTAS.Commands; 

public class TimeCommand : PluginComponent {
    [TasCommand("Time", AliasNames = new[] {"Time:", "Time："}, CalcChecksum = false)]
    private static void Time() {
        // dummy
    }

    private void Awake() {
        HookHelper.ActiveSceneChanged(Finish);
    }

    private static void Finish() {
        if (CurrentSceneName == nameof(Scenes.scene_win) && Level.ScoringData is {} data) {
            MetadataCommand.UpdateAll("Time", command => GameInfoHelper.FormatTime(data.time));
        }
    }
}