using ECommons.Logging;
using Dalamud.Plugin;
using ECommons.DalamudServices;
using ECommons.Reflection;
using System;
using System.Linq;
using System.Reflection;

namespace ECommons.SplatoonAPI;

public static class Splatoon
{
    internal static IDalamudPlugin Instance;
    internal static int Version;

    internal static Action OnConnect;

    internal static void Init()
    {
        try
        {
            if (Svc.PluginInterface.GetIpcSubscriber<bool>("Splatoon.IsLoaded").InvokeFunc())
            {
                Connect();
            }
        }
        catch { }
        Svc.PluginInterface.GetIpcSubscriber<bool>("Splatoon.Loaded").Subscribe(Connect);
        Svc.PluginInterface.GetIpcSubscriber<bool>("Splatoon.Unloaded").Subscribe(Reset);
    }

    /// <summary>
    /// Executed when connected or reconnected to Splatoon. Create and recreate your elements here. Once this event is fired, all already created elements become invalid.
    /// </summary>
    /// <param name="action">Action to be executed on connect</param>
    public static void SetOnConnect(Action action)
    {
        OnConnect = action;
        try
        {
            if (Svc.PluginInterface.GetIpcSubscriber<bool>("Splatoon.IsLoaded").InvokeFunc())
            {
                OnConnect();
            }
        }
        catch { }
    }

    internal static void Shutdown()
    {
        Svc.PluginInterface.GetIpcSubscriber<bool>("Splatoon.Loaded").Unsubscribe(Connect);
        Svc.PluginInterface.GetIpcSubscriber<bool>("Splatoon.Unloaded").Unsubscribe(Reset);
    }

    internal static void Reset()
    {
        Instance = null;
        PluginLog.Information("Disconnected from Splatoon");
    }

    static void Connect()
    {
        try
        {
            if (DalamudReflector.TryGetDalamudPlugin("Splatoon", out var plugin, false, true) && (bool)plugin.GetType().GetField("Init").GetValue(plugin))
            {
                Instance = plugin;
                Version++;
                OnConnect?.Invoke();
                PluginLog.Information("Successfully connected to Splatoon.");
            }
            else
            {
                throw new Exception("Splatoon is not initialized");
            }
        }
        catch (Exception e)
        {
            PluginLog.Error("Can't find Splatoon plugin: " + e.Message);
            PluginLog.Error(e.StackTrace);
        }
    }

    /// <returns>Whether currently connected to Splatoon</returns>
    public static bool IsConnected()
    {
        return Instance != null;
    }

    /// <summary>
    /// Add persistent dynamic element.
    /// </summary>
    /// <param name="name">Non-unique namespace of the element</param>
    /// <param name="e">Element or array of elements</param>
    /// <param name="DestroyCondition">Destroy condition or array of them where: -2 is destroy on zone change; -1 is destroy on combat end; 0 is never destroy; any other number - system tick at which it should be destroyed, where current system tick can be obtained with Environment.TickCount64 parameter.</param>
    /// <returns>Whether operation was successful</returns>
    public static bool AddDynamicElement(string name, Element e, long[] DestroyCondition)
    {
        return AddDynamicElements(name, new Element[] { e }, DestroyCondition);
    }

    /// <summary>
    /// Add persistent dynamic element.
    /// </summary>
    /// <param name="name">Non-unique namespace of the element</param>
    /// <param name="e">Element or array of elements</param>
    /// <param name="DestroyCondition">Destroy condition or array of them where: -2 is destroy on zone change; -1 is destroy on combat end; 0 is never destroy; any other number - system tick at which it should be destroyed, where current system tick can be obtained with Environment.TickCount64 parameter.</param>
    /// <returns>Whether operation was successful</returns>
    public static bool AddDynamicElement(string name, Element e, long DestroyCondition)
    {
        return AddDynamicElements(name, new Element[] { e }, new long[] { DestroyCondition });
    }

    /// <summary>
    /// Add persistent dynamic element.
    /// </summary>
    /// <param name="name">Non-unique namespace of the element</param>
    /// <param name="e">Element or array of elements</param>
    /// <param name="DestroyCondition">Destroy condition or array of them where: -2 is destroy on zone change; -1 is destroy on combat end; 0 is never destroy; any other number - system tick at which it should be destroyed, where current system tick can be obtained with Environment.TickCount64 parameter.</param>
    /// <returns>Whether operation was successful</returns>
    public static bool AddDynamicElements(string name, Element[] e, long DestroyCondition)
    {
        return AddDynamicElements(name, e, new long[] { DestroyCondition });
    }

    /// <summary>
    /// Add persistent dynamic element.
    /// </summary>
    /// <param name="name">Non-unique namespace of the element</param>
    /// <param name="e">Element or array of elements</param>
    /// <param name="DestroyCondition">Destroy condition or array of them where: -2 is destroy on zone change; -1 is destroy on combat end; 0 is never destroy; any other number - amount of seconds before element will be destroyed from the moment of it's addition.</param>
    /// <returns>Whether operation was successful</returns>
    public static bool AddDynamicElement(string name, Element e, float[] DestroyCondition)
    {
        return AddDynamicElements(name, new Element[] { e }, DestroyCondition);
    }

    /// <summary>
    /// Add persistent dynamic element.
    /// </summary>
    /// <param name="name">Non-unique namespace of the element</param>
    /// <param name="e">Element or array of elements</param>
    /// <param name="DestroyCondition">Destroy condition or array of them where: -2 is destroy on zone change; -1 is destroy on combat end; 0 is never destroy; any other number - amount of seconds before element will be destroyed from the moment of it's addition.</param>
    /// <returns>Whether operation was successful</returns>
    public static bool AddDynamicElement(string name, Element e, float DestroyCondition)
    {
        return AddDynamicElements(name, new Element[] { e }, new float[] { DestroyCondition });
    }

    /// <summary>
    /// Add persistent dynamic element.
    /// </summary>
    /// <param name="name">Non-unique namespace of the element</param>
    /// <param name="e">Element or array of elements</param>
    /// <param name="DestroyCondition">Destroy condition or array of them where: -2 is destroy on zone change; -1 is destroy on combat end; 0 is never destroy; any other number - amount of seconds before element will be destroyed from the moment of it's addition.</param>
    /// <returns>Whether operation was successful</returns>
    public static bool AddDynamicElements(string name, Element[] e, float DestroyCondition)
    {
        return AddDynamicElements(name, e, new float[] { DestroyCondition });
    }

    /// <summary>
    /// Add persistent dynamic element.
    /// </summary>
    /// <param name="name">Non-unique namespace of the element</param>
    /// <param name="e">Element or array of elements</param>
    /// <param name="DestroyConditionF">Destroy condition or array of them where: -2 is destroy on zone change; -1 is destroy on combat end; 0 is never destroy; any other number - amount of seconds before element will be destroyed from the moment of it's addition.</param>
    /// <returns>Whether operation was successful</returns>
    public static bool AddDynamicElements(string name, Element[] e, float[] DestroyConditionF)
    {
        var dCond = DestroyConditionF.Select(x => x > 0 ? ((long)(x * 1000f) + Environment.TickCount64) : (long)x).ToArray();
        return AddDynamicElements(name, e, dCond);
    }

    /// <summary>
    /// Add persistent dynamic element.
    /// </summary>
    /// <param name="name">Non-unique namespace of the element</param>
    /// <param name="e">Element or array of elements</param>
    /// <param name="DestroyCondition">Destroy condition or array of them where: -2 is destroy on zone change; -1 is destroy on combat end; 0 is never destroy; any other number - system tick at which it should be destroyed, where current system tick can be obtained with Environment.TickCount64 parameter.</param>
    /// <returns>Whether operation was successful</returns>
    public static bool AddDynamicElements(string name, Element[] e, long[] DestroyCondition)
    {
        if (!IsConnected())
        {
            PluginLog.Warning("Not connected to Splatoon");
            return false;
        }
        if (!e.All(x => x.IsValid()))
        {
            PluginLog.Warning("Elements are no longer valid");
            return false;
        }
        if (e.Length == 0)
        {
            PluginLog.Warning("There are no elements");
            return false;
        }
        try
        {
            var array = Array.CreateInstance(e[0].Instance.GetType(), e.Length);
            for (var i = 0; i < e.Length; i++)
            {
                array.SetValue(e[i].Instance, i);
            }
            Instance.GetType().GetMethod("AddDynamicElements").Invoke(Instance, new object[] { name, array, DestroyCondition });
            return true;
        }
        catch (Exception ex)
        {
            ex.Log();
            return false;
        }
    }

    /// <summary>
    /// Display certain element in next frame only. 
    /// </summary>
    /// <param name="e">Element to display</param>
    /// <returns>Whether operation was successful</returns>
    public static bool DisplayOnce(Element e)
    {
        if (!IsConnected())
        {
            PluginLog.Warning("Not connected to Splatoon");
            return false;
        }
        if (!e.IsValid())
        {
            PluginLog.Warning("Elements are no longer valid");
            return false;
        }
        try
        {
            Instance.GetType().GetMethod("InjectElement").Invoke(Instance, new object[] { e.Instance });
            return true;
        }
        catch (Exception ex)
        {
            ex.Log();
            return false;
        }
    }

    /// <summary>
    /// Removes dynamic elements with specific name. If more than one element was registered under certain name, all of these elements will be removed.
    /// </summary>
    /// <param name="name">Name of element(s)</param>
    /// <returns>Whether operation was successful</returns>
    public static bool RemoveDynamicElements(string name)
    {
        if (!IsConnected())
        {
            PluginLog.Warning("Not connected to Splatoon");
            return false;
        }
        try
        {
            Instance.GetType().GetMethod("RemoveDynamicElements").Invoke(Instance, new object[] { name });
            return true;
        }
        catch (Exception ex)
        {
            ex.Log();
            return false;
        }
    }

    /// <summary>
    /// Attempts to decode element that was encoded into JSON.
    /// </summary>
    /// <param name="input">Input string, you can export it from Splatoon. Make sure element is enabled!</param>
    /// <returns>Element that is ready for use or null if failed</returns>
    public static Element DecodeElement(string input)
    {
        var method = Instance.GetType().Assembly.GetType("Splatoon.SplatoonScripting.ScriptingEngine", true).GetMethod("TryDecodeElement", BindingFlags.Public | BindingFlags.Static);
        var parameters = new object[] { input, null };
        var result = (bool)method.Invoke(null, parameters);
        if (result)
        {
            return new Element(parameters[1]);
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Work in progress on this function
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [Obsolete("Work in progress")]
    public static object DecodeLayout(string input)
    {
        var method = Instance.GetType().Assembly.GetType("Splatoon.SplatoonScripting.ScriptingEngine", true).GetMethod("TryDecodeLayout", BindingFlags.Public | BindingFlags.Static);
        var parameters = new object[] { input, null };
        var result = (bool)method.Invoke(null, parameters);
        if (result)
        {
            return parameters[1];
        }
        else
        {
            return null;
        }
    }
}
