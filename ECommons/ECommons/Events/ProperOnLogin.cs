using ECommons.Logging;
using ECommons.DalamudServices;
using System;
using System.Collections.Generic;

namespace ECommons.Events;

public static class ProperOnLogin
{
    static HashSet<Action> RegisteredActions = new();
    static bool EventRegistered = false;

    public static bool PlayerPresent => Svc.ClientState.LocalPlayer != null && Svc.ClientState.LocalContentId != 0;

    public static void Register(Action action, bool fireImmediately = false)
    {
        if (RegisteredActions.Contains(action))
        {
            PluginLog.Warning($"{action.GetType().FullName} ProperOnLogin event is already registered!");
        }
        else
        {
            RegisteredActions.Add(action);
            PluginLog.Debug($"Registered ProperOnLogin event: {action.GetType().FullName}");
            if (!EventRegistered)
            {
                EventRegistered = true;
                Svc.ClientState.Login += OnLogin;
                PluginLog.Debug("ProperOnLogin master event registered");
            }
            if(fireImmediately && PlayerPresent)
            {
                GenericHelpers.Safe(action);
            }
        }
    }

    [Obsolete("This event is automatically disposed together with ECommons")]
    public static void Unregister(Action action)
    {
        if (!RegisteredActions.Contains(action))
        {
            PluginLog.Warning($"{action.GetType().FullName} ProperOnLogin event is not registered!");
        }
        else
        {
            RegisteredActions.Remove(action);
            PluginLog.Debug($"Unregistered ProperOnLogin event: {action.GetType().FullName}");
        }
    }

    static void OnLogin()
    {
        Svc.Framework.Update += OnUpdate;
        PluginLog.Debug("Registering ProperOnLogin event's framework update");
    }

    static void OnUpdate(object _)
    {
        if(PlayerPresent)
        {
            PluginLog.Debug("Firing ProperOnLogin event and unregistering framework update");
            Svc.Framework.Update -= OnUpdate;
            foreach(var x in RegisteredActions)
            {
                try
                {
                    x();
                }
                catch(Exception e)
                {
                    PluginLog.Error($"Exception while processing ProperOnLogin event in {x.GetType().FullName}");
                    e.Log();
                }
            }
        }
    }

    internal static void Dispose()
    {
        if (EventRegistered)
        {
            Svc.ClientState.Login -= OnLogin;
            PluginLog.Debug("ProperOnLogin master event unregistered");
        }
    }
}
