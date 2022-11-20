using Rewired;
using UnityEngine;

namespace CupheadTAS.Components;

public class IgnoreInputWhenLoseFocus : PluginComponent {
    private void Awake() {
        Application.runInBackground = true;
    }

    private void Update() {
        if (ReInput.configuration != null) {
            ReInput.configuration.ignoreInputWhenAppNotInFocus = true;
            Destroy(this);
        }
    }
}
