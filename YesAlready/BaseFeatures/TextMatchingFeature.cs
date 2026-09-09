using Dalamud.Game.ClientState.Conditions;
using ECommons.GameHelpers;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace YesAlready.BaseFeatures;

public abstract class TextMatchingFeature : AddonFeature
{
    protected abstract unsafe string GetSetLastSeenText(AtkUnitBase* atk);
    protected abstract unsafe object? ShouldProceed(string text, AtkUnitBase* atk);
    protected abstract unsafe void Proceed(AtkUnitBase* atk, object? matchingNode = null);

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active) return;

        var atk = addonInfo.GetAddon<AtkUnitBase>();
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
            var msg = matchingNode switch
            {
                ITextNode { Name: { Length: > 0 } name } => $"Matched on [{name}] -> [{text}]",
                int index when Service.Watcher.LastSelectedListEntry?.Node is { Name: { Length: > 0 } name } => $"Matched [{name}] -> [{GetListEntryText(index, text)}]",
                int index => $"Matched on index #{index} -> [{GetListEntryText(index, text)}]",
                _ => $"Matched on {text}",
            };
            Log(msg);
            Proceed(atk, matchingNode);
        }
        else
            Log("Not proceeding");
    }

    private static string GetListEntryText(int index, string fallback)
        => index >= 0 && index < Service.Watcher.LastSeenListEntries.Length ? Service.Watcher.LastSeenListEntries[index].Text : fallback;

    protected bool EntryMatchesText(string pattern, string text, bool isRegex)
    {
        if (string.IsNullOrEmpty(pattern)) return false;

        pattern = pattern.NormalizeForMatch();
        text = text.NormalizeForMatch();

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
        else if (text.ContainsIgnoreWhitespace(pattern))
        {
            LogVerbose($"Matched on text {pattern} ({text})");
            return true;
        }
        LogVerbose($"No match on {pattern} ({text})");
        return false;
    }

    protected int? GetMatchingIndex(string pattern, string text, bool isRegex)
    {
        pattern = pattern.NormalizeForMatch();
        text = text.NormalizeForMatch();

        if (isRegex)
        {
            try
            {
                var regex = new Regex(pattern.Trim('/'), RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var match = regex.Match(text);
                if (match.Success)
                {
                    LogVerbose($"Matched on regex {pattern} ({text})");
                    return match.Index;
                }
            }
            catch (Exception ex)
            {
                LogError($"Invalid regex pattern {pattern}: {ex.Message}");
                return null;
            }
        }
        else
        {
            var index = text.WithoutWhitespace().IndexOf(pattern.WithoutWhitespace(), StringComparison.OrdinalIgnoreCase);
            if (index != -1)
            {
                LogVerbose($"Matched on text {pattern} ({text})");
                return index;
            }
        }
        LogVerbose($"No match on {pattern} ({text})");
        return null;
    }

    protected int? GetMatchingIndex(string[] entries, string pattern, bool isRegex)
    {
        for (var i = 0; i < entries.Length; i++)
        {
            if (EntryMatchesText(pattern, entries[i], isRegex))
                return i;
        }
        return null;
    }

    private void SetEntry()
    {
        try
        {
            Service.Watcher.LastSeenListSelection = Service.Watcher.LastSeenListIndex < Service.Watcher.LastSeenListEntries.Length ? Service.Watcher.LastSeenListEntries?[Service.Watcher.LastSeenListIndex].Text ?? string.Empty : string.Empty;
            Service.Watcher.LastSeenListTarget = Service.Watcher.LastSeenListTarget = Svc.Targets.Target != null ? Svc.Targets.Target.Name.GetText() ?? string.Empty : string.Empty;
        }
        catch { }
    }

    protected class LastEntry
    {
        public uint TargetDataId { get; set; }
        public string EntryText { get; set; } = string.Empty;
    }

    protected bool CheckRestrictions(ITextNode node)
    {
        if (node is IZoneRestrictedNode { ZoneRestricted: true } zoneNode)
        {
            if (Player.Territory is { ValueNullable.PlaceName.ValueNullable.Name: { IsEmpty: false } name })
            {
                if (!EntryMatchesText(zoneNode.ZoneText, name.ToString(), zoneNode.ZoneIsRegex))
                {
                    Log($"Zone restriction not met: {name} does not match {zoneNode.ZoneText}");
                    return false;
                }
            }
        }

        if (node is ITargetRestrictedNode { TargetRestricted: true } targetNode)
        {
            if (Svc.Targets.Target is { Name: var name })
            {
                if (!EntryMatchesText(targetNode.TargetText, name.ToString(), targetNode.TargetIsRegex))
                {
                    Log($"Target restriction not met: {name} does not match {targetNode.TargetText}");
                    return false;
                }
            }
            else
            {
                Log($"Target restriction not met: No target selected");
                return false;
            }
        }

        if (node is IPlayerConditionRestrictedNode { RequiresPlayerConditions: true } playerConditionNode)
        {
            var conditions = playerConditionNode.PlayerConditions.Replace(" ", "").Split(',');
            Log($"[{nameof(IPlayerConditionRestrictedNode)}] Conditions: {string.Join(", ", conditions)}");
            if (!conditions.All(condition => Enum.TryParse<ConditionFlag>(condition.StartsWith('!') ? condition[1..] : condition, out var flag) && (condition.StartsWith('!') ? !Svc.Condition[flag] : Svc.Condition[flag])))
            {
                Log($"Matched on {node.Name}, but not all conditions were met");
                return false;
            }
        }

        if (node is INumberRestrictedNode { IsConditional: true } numberNode)
        {
            if (numberNode.ConditionalNumberRegex?.IsMatch(node.Name) ?? false)
            {
                PluginLog.Debug("AddonSelectYesNo: Is conditional matches");
                if (numberNode.ConditionalNumberRegex?.Match(node.Name) is { Success: true, Value: var result } && int.TryParse(result, out var value))
                {
                    PluginLog.Debug($"AddonSelectYesNo: Is conditional - {value}");
                    return numberNode.ComparisonType switch
                    {
                        ComparisonType.LessThan => value < numberNode.ConditionalNumber,
                        ComparisonType.GreaterThan => value > numberNode.ConditionalNumber,
                        ComparisonType.LessThanOrEqual => value <= numberNode.ConditionalNumber,
                        ComparisonType.GreaterThanOrEqual => value >= numberNode.ConditionalNumber,
                        ComparisonType.Equal => value == numberNode.ConditionalNumber,
                        _ => throw new Exception("Uncaught enum"),
                    };
                }
            }

            return false;
        }

        return true;
    }
}
