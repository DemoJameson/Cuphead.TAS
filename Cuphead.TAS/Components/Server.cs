using BepInEx.Configuration;
using TAS.Core;

namespace CupheadTAS.Components; 

public class Server : PluginComponent {
    private ConfigEntry<int> port;

    private void Awake() {
        port = Plugin.Instance.Config.Bind("General", "Communication Server Port", 19982);
        port.SettingChanged += (_, _) => {
            CommunicationServer.Start(port.Value);
        };
        CommunicationServer.Start(port.Value);
    }

    private void OnDestroy() {
        CommunicationServer.Stop();
    }
}