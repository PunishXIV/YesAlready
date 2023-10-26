using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using ECommons.Logging;
using ECommons.DalamudServices;
using System;
using System.Collections.Generic;

namespace ECommons.ObjectLifeTracker;

public static class ObjectLife
{
    delegate IntPtr GameObject_ctor(IntPtr obj);
    static Hook<GameObject_ctor> GameObject_ctor_hook = null;
    static Dictionary<IntPtr, long> GameObjectLifeTime = null;
    public static Action<nint> OnObjectCreation = null;

    internal static void Init()
    {
        GameObjectLifeTime = new();
#pragma warning disable CS0618 // Type or member is obsolete
        GameObject_ctor_hook = Svc.Hook.HookFromAddress<GameObject_ctor>(Svc.SigScanner.ScanText("48 8D 05 ?? ?? ?? ?? C7 81 ?? ?? ?? ?? ?? ?? ?? ?? 48 89 01 48 8B C1 C3"), GameObject_ctor_detour);
#pragma warning restore CS0618 // Type or member is obsolete
        GameObject_ctor_hook.Enable();
        foreach (var x in Svc.Objects)
        {
            GameObjectLifeTime[x.Address] = Environment.TickCount64;
        }
    }

    internal static void Dispose()
    {
        if (GameObject_ctor_hook != null)
        {
            GameObject_ctor_hook.Disable();
            GameObject_ctor_hook.Dispose();
            GameObject_ctor_hook = null;
        }
        GameObjectLifeTime = null;
    }

    static IntPtr GameObject_ctor_detour(IntPtr ptr)
    {
        if (GameObjectLifeTime == null)
        {
            throw new Exception("GameObjectLifeTime is null. Have you initialised the ObjectLife module on ECommons initialisation?");
        }
        GameObjectLifeTime[ptr] = Environment.TickCount64;
        var ret = GameObject_ctor_hook.Original(ptr);

        if (OnObjectCreation != null)
        {
            try
            {
                OnObjectCreation(ptr);
            }
            catch (Exception e)
            {
                e.Log($"Exception in GameObject_ctor_detour");
            }
        }
        return ret;
    }

    public static long GetLifeTime(this GameObject o)
    {
        return Environment.TickCount64 - GetSpawnTime(o);
    }

    public static float GetLifeTimeSeconds(this GameObject o)
    {
        return (float)o.GetLifeTime() / 1000f;
    }

    public static long GetSpawnTime(this GameObject o)
    {
        if (GameObject_ctor_hook == null) throw new Exception("Object life tracker was not initialized");
        if (GameObjectLifeTime.TryGetValue(o.Address, out var result))
        {
            return result;
        }
        else
        {
            PluginLog.Warning($"Warning: object life data could not be found\n" +
                $"Object addr: {o.Address:X16} ID: {o.ObjectId:X8} Name: {o.Name}");
            return 0;
        }
    }
}
