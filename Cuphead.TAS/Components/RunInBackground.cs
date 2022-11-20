using UnityEngine;

namespace CupheadTAS.Components;

public class RunInBackground : PluginComponent {
    private void Update() {
        Application.runInBackground = true;
    }
}