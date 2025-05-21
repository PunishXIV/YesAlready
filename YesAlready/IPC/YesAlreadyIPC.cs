using ECommons.EzIpcManager;
using System;

namespace YesAlready.IPC;

public class YesAlreadyIPC
{
    public YesAlreadyIPC() => EzIPC.Init(this);

    [EzIPC] public bool IsPluginEnabled() => P.Active;

    [EzIPC] public void SetPluginEnabled(bool state) => C.Enabled = state;

    [EzIPC] public bool IsBotherEnabled(string name) => GetFeature(name) is { Enabled: true };

    [EzIPC]
    public void SetBotherEnabled(string name, bool state)
    {
        if (state)
            GetFeature(name)?.Enable();
        else
            GetFeature(name)?.Disable();
    }

    [EzIPC]
    public void PausePlugin(int milliseconds)
    {
        C.Enabled = false;
        Service.TaskManager.EnqueueDelay(milliseconds);
        Service.TaskManager.Enqueue(() => C.Enabled = true);
    }

    [EzIPC]
    public bool PauseBother(string name, int milliseconds)
    {
        var feature = GetFeature(name);
        if (feature is null || !feature.Enabled)
            return false;
        feature.Disable();
        Service.TaskManager.EnqueueDelay(milliseconds);
        Service.TaskManager.Enqueue(feature.Enable);
        return true;
    }

    private BaseFeature? GetFeature(string name)
        => Type.GetType(name) is { } type && typeof(BaseFeature).IsAssignableFrom(type) && !type.IsAbstract
        ? (BaseFeature?)Activator.CreateInstance(type)
        : null;
}
