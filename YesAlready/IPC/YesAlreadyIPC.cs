namespace YesAlready.IPC;

internal static class YesAlreadyIPC
{
    internal static void Init() => Svc.PluginInterface.GetIpcProvider<bool, object>("YesAlready.SetPluginEnabled").RegisterAction(SetPluginEnabled);

    internal static void Dispose() => Svc.PluginInterface.GetIpcProvider<bool, object>("YesAlready.SetPluginEnabled").UnregisterAction();

    private static void SetPluginEnabled(bool state) => P.Config.Enabled = state;
}
