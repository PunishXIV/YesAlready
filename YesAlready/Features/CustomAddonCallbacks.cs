using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons.Automation;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;
public class CustomAddonCallbacks : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        foreach (var addon in P.Config.CustomCallbacks)
            Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, addon.Addon, AddonSetup);
    }

    public override void Disable()
    {
        base.Disable();
        Svc.AddonLifecycle.UnregisterListener(AddonSetup);
    }

    protected static unsafe void AddonSetup(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active) return;

        var addon = addonInfo.Base();
        var callbacks = CallbackToArray(P.Config.CustomCallbacks.First(x => x.Addon == addonInfo.AddonName).CallbackParams);
        Callback.Fire(addon, true, callbacks);
    }

    public static string CallbackToString(object[] args)
    {
        var sb = new StringBuilder();
        foreach (var obj in args)
        {
            if (obj is uint)
                sb.Append('u');
            else if (obj is string str && str.Contains(' '))
                sb.Append($"\"{str}\"");
            else
                sb.Append(obj.ToString());
            sb.Append(' ');
        }
        if (sb.Length >= 2)
            sb.Length -= 2;
        return sb.ToString();
    }

    public static object[] CallbackToArray(string args)
    {
        var rawValues = args.Split(' ');
        var valueArgs = new List<object>();

        var current = "";
        var inQuotes = false;

        for (var i = 0; i < rawValues.Length; i++)
        {
            if (!inQuotes)
            {
                if (rawValues[i].StartsWith('\"'))
                {
                    inQuotes = true;
                    current = rawValues[i].TrimStart('"');
                }
                else
                {
                    if (int.TryParse(rawValues[i], out var iValue)) valueArgs.Add(iValue);
                    else if (uint.TryParse(rawValues[i].TrimEnd('U', 'u'), out var uValue)) valueArgs.Add(uValue);
                    else if (bool.TryParse(rawValues[i], out var bValue)) valueArgs.Add(bValue);
                    else valueArgs.Add(rawValues[i]);
                }
            }
            else
            {
                if (rawValues[i].EndsWith('\"'))
                {
                    inQuotes = false;
                    current += " " + rawValues[i].TrimEnd('"');
                    valueArgs.Add(current);
                    current = "";
                }
                else
                    current += " " + rawValues[i];
            }
        }
        return [.. valueArgs];
    }
}
