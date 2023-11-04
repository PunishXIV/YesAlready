using Dalamud.Hooking;
using ECommons.Logging;
using Dalamud.Utility.Signatures;
using ECommons.DalamudServices;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommons.Hooks
{
    public static class DirectorUpdate
    {
        const string Sig = "48 89 5C 24 ?? 57 48 83 EC 30 41 8B D9";

        public delegate long ProcessDirectorUpdate(long a1, long a2, DirectorUpdateCategory a3, uint a4, uint a5, int a6, int a7);
        internal static Hook<ProcessDirectorUpdate> ProcessDirectorUpdateHook = null;
        static Action<long, long, DirectorUpdateCategory, uint, uint, int, int> FullParamsCallback = null;
        static Action<DirectorUpdateCategory> CategoryOnlyCallback = null;

        internal static long ProcessDirectorUpdateDetour_Full(long a1, long a2, DirectorUpdateCategory a3, uint a4, uint a5, int a6, int a7)
        {
            try
            {
                FullParamsCallback(a1, a2, a3, a4, a5, a6, a7);
            }
            catch (Exception e)
            {
                e.Log();
            }
            return ProcessDirectorUpdateHook.Original(a1, a2, a3, a4, a5, a6, a7);
        }

        internal static long ProcessDirectorUpdateDetour_Category(long a1, long a2, DirectorUpdateCategory a3, uint a4, uint a5, int a6, int a7)
        {
            try
            {
                CategoryOnlyCallback(a3);
            }
            catch (Exception e)
            {
                e.Log();
            }
            return ProcessDirectorUpdateHook.Original(a1, a2, a3, a4, a5, a6, a7);
        }

        public static void Init(Action<long, long, DirectorUpdateCategory, uint, uint, int, int> fullParamsCallback)
        {
            if(ProcessDirectorUpdateHook != null)
            {
                throw new Exception("Director Update Hook is already initialized!");
            }
            if (Svc.SigScanner.TryScanText(Sig, out var ptr))
            {
                FullParamsCallback = fullParamsCallback;
                ProcessDirectorUpdateHook = Svc.Hook.HookFromAddress<ProcessDirectorUpdate>(ptr, ProcessDirectorUpdateDetour_Full);
                Enable();
                PluginLog.Information($"Requested Director Update hook and successfully initialized with FULL data");
            }
            else
            {
                PluginLog.Error($"Could not find DirectorUpdate signature");
            }
        }

        public static void Init(Action<DirectorUpdateCategory> categoryOnlyCallback)
        {
            if (ProcessDirectorUpdateHook != null)
            {
                throw new Exception("Director Update Hook is already initialized!");
            }
            if (Svc.SigScanner.TryScanText(Sig, out var ptr))
            {
                CategoryOnlyCallback = categoryOnlyCallback;
                ProcessDirectorUpdateHook = Svc.Hook.HookFromAddress<ProcessDirectorUpdate>(ptr, ProcessDirectorUpdateDetour_Category);
                Enable();
                PluginLog.Information($"Requested Director Update hook and successfully initialized with CATEGORY ONLY data");
            }
            else
            {
                PluginLog.Error($"Could not find DirectorUpdate signature");
            }
        }

        public static void Enable()
        {
            if (ProcessDirectorUpdateHook?.IsEnabled == false) ProcessDirectorUpdateHook?.Enable();
        }

        public static void Disable()
        {
            if (ProcessDirectorUpdateHook?.IsEnabled == true) ProcessDirectorUpdateHook?.Disable();
        }

        public static void Dispose()
        {
            if(ProcessDirectorUpdateHook != null)
            {
                PluginLog.Information($"Disposing Director Update Hook");
                Disable();
                ProcessDirectorUpdateHook?.Dispose();
                ProcessDirectorUpdateHook = null;
            }
        }
    }
}
