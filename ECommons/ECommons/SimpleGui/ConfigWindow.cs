using Dalamud.Interface.Windowing;
using ECommons.Configuration;
using ECommons.DalamudServices;
using ECommons.ImGuiMethods;

namespace ECommons.SimpleGui;

internal class ConfigWindow : Window
{
    public ConfigWindow(string name) : base(name)
    {
        this.SizeConstraints = new()
        {
            MinimumSize = new(200, 200),
            MaximumSize = new(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw()
    {
        GenericHelpers.Safe(EzConfigGui.Draw);
    }

    public override void OnOpen()
    {
        EzConfigGui.OnOpen?.Invoke();
    }

    public override void OnClose()
    {
        if(EzConfigGui.Config != null)
        {
            Svc.PluginInterface.SavePluginConfig(EzConfigGui.Config);
            Notify.Success("Configuration saved");
        }
        if(EzConfig.Config != null)
        {
            EzConfig.Save();
            Notify.Success("Configuration saved");
        }
        EzConfigGui.OnClose?.Invoke();
    }
}
