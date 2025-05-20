using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace YesAlready.BaseFeatures;

public abstract class TextMatchingFeature : AddonFeature
{
    protected override bool IsEnabled() => true;
    protected abstract unsafe string GetSetLastSeenText(AtkUnitBase* atk);
    protected abstract unsafe object? ShouldProceed(string text, AtkUnitBase* atk);
    protected abstract unsafe void Proceed(AtkUnitBase* atk, object? matchingNode = null);

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk)
    {
        if (!P.Active) return;

        if (eventType is AddonEvent.PreFinalize && addonInfo.AddonName is "SelectString" or "SelectIconString")
        {
            SetEntry();
            return;
        }

        if (!GenericHelpers.IsAddonReady(atk))
        {
            if (addonInfo.AddonName is "Talk") return; // don't bother logging this
            Log("Addon not ready");
            return;
        }

        var text = GetSetLastSeenText(atk);
        Log($"text={text}");

        if (ShouldProceed(text, atk) is { } matchingNode)
        {
            Log("Proceeding");
            Proceed(atk, matchingNode);
        }
        else
            Log("Not proceeding");
    }

    protected bool EntryMatchesText(string pattern, string text, bool isRegex)
    {
        if (string.IsNullOrEmpty(pattern)) return false;
        if (isRegex)
        {
            try
            {
                var regex = new Regex(pattern.Trim('/'), RegexOptions.Compiled | RegexOptions.IgnoreCase);
                if (regex.IsMatch(text))
                {
                    LogVerbose($"Matched on regex {pattern} ({text})");
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogError($"Invalid regex pattern {pattern}: {ex.Message}");
                return false;
            }
        }
        else if (text.Contains(pattern))
        {
            LogVerbose($"Matched on text {pattern} ({text})");
            return true;
        }
        LogVerbose($"No match on {pattern} ({text})");
        return false;
    }

    protected void Log(string message) => PluginLog.Debug($"[{GetType().Name}]: {message}");
    protected void LogVerbose(string message) => PluginLog.Verbose($"[{GetType().Name}]: {message}");
    protected void LogError(string message) => PluginLog.Error($"[{GetType().Name}]: {message}");

    protected int? GetMatchingIndex(string[] entries)
    {
        var nodes = P.Config.GetAllNodes().OfType<ListEntryNode>();
        foreach (var node in nodes)
        {
            if (!node.Enabled || string.IsNullOrEmpty(node.Text))
                continue;

            for (var i = 0; i < entries.Length; i++)
            {
                if (EntryMatchesText(node.Text, entries[i], node.IsTextRegex))
                    return i;
            }
        }
        return null;
    }

    private void SetEntry()
    {
        try
        {
            P.LastSeenListSelection = P.LastSeenListIndex < P.LastSeenListEntries.Length ? P.LastSeenListEntries?[P.LastSeenListIndex].Text ?? string.Empty : string.Empty;
            P.LastSeenListTarget = P.LastSeenListTarget = Svc.Targets.Target != null ? Svc.Targets.Target.Name.GetText() ?? string.Empty : string.Empty;
        }
        catch { }
    }
}
