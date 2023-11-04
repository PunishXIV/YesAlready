using Dalamud.Hooking;
using ECommons.DalamudServices;
using ECommons.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommons.Hooks
{
    public static class SendAction
    {
        const string Sig = "E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? F3 0F 10 3D ?? ?? ?? ?? 48 8D 4D BF";

        public delegate long SendActionDelegate(long targetObjectId, byte actionType, uint actionId, ushort sequence, long a5, long a6, long a7, long a8, long a9);
        public delegate void SendActionCallbackDelegate(long targetObjectId, byte actionType, uint actionId, ushort sequence, long a5, long a6, long a7, long a8, long a9);
        internal static Hook<SendActionDelegate> SendActionHook = null;
        static SendActionCallbackDelegate Callback = null;

        internal static long SendActionDetour(long targetObjectId, byte actionType, uint actionId, ushort sequence, long a5, long a6, long a7, long a8, long a9)
        {
            try
            {
                Callback(targetObjectId, actionType, actionId, sequence, a5, a6, a7, a8, a9);
            }
            catch (Exception e)
            {
                e.Log();
            }
            return SendActionHook.Original(targetObjectId, actionType, actionId, sequence, a5, a6, a7, a8, a9);
        }

        public static void Init(SendActionCallbackDelegate fullParamsCallback)
        {
            if (SendActionHook != null)
            {
                throw new Exception("SendAction Hook is already initialized!");
            }
            if (Svc.SigScanner.TryScanText(Sig, out var ptr))
            {
                Callback = fullParamsCallback;
                SendActionHook = Svc.Hook.HookFromAddress<SendActionDelegate>(ptr, SendActionDetour);
                Enable();
                PluginLog.Information($"Requested SendAction hook and successfully initialized");
            }
            else
            {
                PluginLog.Error($"Could not find SendAction signature");
            }
        }

        public static void Enable()
        {
            if (SendActionHook?.IsEnabled == false) SendActionHook?.Enable();
        }

        public static void Disable()
        {
            if (SendActionHook?.IsEnabled == true) SendActionHook?.Disable();
        }

        public static void Dispose()
        {
            if (SendActionHook != null)
            {
                PluginLog.Information($"Disposing SendAction Hook");
                Disable();
                SendActionHook?.Dispose();
                SendActionHook = null;
            }
        }
    }
}
