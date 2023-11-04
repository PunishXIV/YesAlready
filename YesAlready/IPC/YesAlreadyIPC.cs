using ECommons.DalamudServices;

namespace YesAlready.IPC
{
    internal static class YesAlreadyIPC
    {
        internal static void Init()
        {
            Svc.PluginInterface.GetIpcProvider<bool, object>("YesAlready.SetPluginEnabled").RegisterAction(SetPluginEnabled);
        }

        internal static void Dispose()
        {
            Svc.PluginInterface.GetIpcProvider<bool, object>("YesAlready.SetPluginEnabled").UnregisterAction();

        }

        private static void SetPluginEnabled(bool state)
        {
            if (state)
                P.Config.Enabled = true;
            else
                P.Config.Enabled = false;
        }
    }
}
