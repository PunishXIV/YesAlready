using Dalamud;
using Dalamud.Game.ClientState.Keys;
using ECommons.Logging;
using Dalamud.Plugin;
using ECommons.DalamudServices;
using ECommons.Schedulers;
using System;
using System.Collections.Generic;
using System.Reflection;
using Dalamud.Common;

namespace ECommons.Reflection;

public static class DalamudReflector
{
    delegate ref int GetRefValue(int vkCode);
    static GetRefValue getRefValue;
    static Dictionary<string, IDalamudPlugin> pluginCache;
    static List<Action> onPluginsChangedActions;

    internal static void Init()
    {
        onPluginsChangedActions = new();
        pluginCache = new();
        GenericHelpers.Safe(delegate
        {
            getRefValue = (GetRefValue)Delegate.CreateDelegate(typeof(GetRefValue), Svc.KeyState,
                        Svc.KeyState.GetType().GetMethod("GetRefValue",
                        BindingFlags.NonPublic | BindingFlags.Instance,
                        null, new Type[] { typeof(int) }, null));
        });
        Svc.PluginInterface.ActivePluginsChanged += OnInstalledPluginsChanged;
    }

    internal static void Dispose()
    {
        if (pluginCache != null)
        {
            pluginCache = null;
            onPluginsChangedActions = null;
        }
        Svc.PluginInterface.ActivePluginsChanged -= OnInstalledPluginsChanged;
    }

    public static void RegisterOnInstalledPluginsChangedEvents(params Action[] actions)
    {
        foreach(var x in actions)
        {
            onPluginsChangedActions.Add(x);
        }
    }

    public static void SetKeyState(VirtualKey key, int state)
    {
        getRefValue((int)key) = state;
    }

    public static object GetPluginManager()
    {
        return Svc.PluginInterface.GetType().Assembly.
                GetType("Dalamud.Service`1", true).MakeGenericType(Svc.PluginInterface.GetType().Assembly.GetType("Dalamud.Plugin.Internal.PluginManager", true)).
                GetMethod("Get").Invoke(null, BindingFlags.Default, null, Array.Empty<object>(), null);
    }

    public static object GetService(string serviceFullName)
    {
        return Svc.PluginInterface.GetType().Assembly.
                GetType("Dalamud.Service`1", true).MakeGenericType(Svc.PluginInterface.GetType().Assembly.GetType(serviceFullName, true)).
                GetMethod("Get").Invoke(null, BindingFlags.Default, null, Array.Empty<object>(), null);
    }

    public static bool TryGetLocalPlugin(out object localPlugin, out Type type)
    {
        try
        {
            if (ECommonsMain.Instance == null)
            {
                throw new Exception("PluginInterface is null. Did you initalise ECommons?");
            }
            var pluginManager = GetPluginManager();
            var installedPlugins = (System.Collections.IList)pluginManager.GetType().GetProperty("InstalledPlugins").GetValue(pluginManager);

            foreach (var t in installedPlugins)
            {
                if (t != null)
                {
                    type = t.GetType().Name == "LocalDevPlugin" ? t.GetType().BaseType : t.GetType();
                    if (object.ReferenceEquals(type.GetField("instance", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(t), ECommonsMain.Instance))
                    {
                        localPlugin = t;
                        return true;
                    }
                }
            }
            localPlugin = type = null;
            return false;
        }
        catch(Exception e)
        {
            e.Log();
            localPlugin = type = null;
            return false;
        }
    }

    public static bool TryGetDalamudPlugin(string internalName, out IDalamudPlugin instance, bool suppressErrors = false, bool ignoreCache = false)
    {
        if (pluginCache == null)
        {
            throw new Exception("PluginCache is null. Have you initialised the DalamudReflector module on ECommons initialisation?");
        }

        if(!ignoreCache && pluginCache.TryGetValue(internalName, out instance) && instance != null)
        {
            return true;
        }
        try
        {
            var pluginManager = GetPluginManager();
            var installedPlugins = (System.Collections.IList)pluginManager.GetType().GetProperty("InstalledPlugins").GetValue(pluginManager);

            foreach (var t in installedPlugins)
            {
                if ((string)t.GetType().GetProperty("Name").GetValue(t) == internalName)
                {
                    var type = t.GetType().Name == "LocalDevPlugin" ? t.GetType().BaseType : t.GetType();
                    var plugin = (IDalamudPlugin)type.GetField("instance", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(t);
                    if (plugin == null)
                    {
                        InternalLog.Warning($"Found requested plugin {internalName} but it was null");
                    }
                    else
                    {
                        instance = plugin;
                        pluginCache[internalName] = plugin;
                        return true;
                    }
                }
            }
            instance = null;
            return false;
        }
        catch (Exception e)
        {
            if (!suppressErrors)
            {
                PluginLog.Error($"Can't find {internalName} plugin: " + e.Message);
                PluginLog.Error(e.StackTrace);
            }
            instance = null;
            return false;
        }
    }
    
    public static bool TryGetDalamudStartInfo(out DalamudStartInfo dalamudStartInfo, DalamudPluginInterface pluginInterface = null)
    {
        try
        {
            if (pluginInterface == null) pluginInterface = Svc.PluginInterface;
            var info = pluginInterface.GetType().Assembly.
                    GetType("Dalamud.Service`1", true).MakeGenericType(Svc.PluginInterface.GetType().Assembly.GetType("Dalamud.Dalamud", true)).
                    GetMethod("Get").Invoke(null, BindingFlags.Default, null, Array.Empty<object>(), null);
            dalamudStartInfo = info.GetFoP<DalamudStartInfo>("StartInfo");
            return true;
        }
        catch (Exception e)
        {
            PluginLog.Error($"{e.Message}\n{e.StackTrace ?? ""}");
            dalamudStartInfo = default;
            return false;
        }
    }

    public static string GetPluginName()
    {
        return Svc.PluginInterface?.InternalName ?? "Not initialized";
    }

    internal static void OnInstalledPluginsChanged(PluginListInvalidationKind kind, bool affectedThisPlugin)
    {
        PluginLog.Verbose("Installed plugins changed event fired");
        _ = new TickScheduler(delegate
        {
            pluginCache.Clear();
            foreach(var x in onPluginsChangedActions)
            {
                x();
            }
        });
    }
}
