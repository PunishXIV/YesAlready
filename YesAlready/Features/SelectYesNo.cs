using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class SelectYesNo : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "SelectYesno", AddonSetup);
    }

    public override void Disable()
    {
        base.Disable();
        Svc.AddonLifecycle.UnregisterListener(AddonSetup);
    }

    protected unsafe void AddonSetup(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active) return;

        var addon = new AddonMaster.SelectYesno(addonInfo.Base());

        var text = P.LastSeenDialogText = addon.TextLegacy;
        PluginLog.Debug($"AddonSelectYesNo: text={text}");

        if (P.ForcedYesKeyPressed)
        {
            PluginLog.Debug($"AddonSelectYesNo: Forced yes hotkey pressed");
            addon.Yes();
            return;
        }

        if (P.Config.GimmickYesNo && Svc.Data.GetExcelSheet<GimmickYesNo>().Where(x => !x.Unknown0.IsEmpty).Select(x => x.Unknown0).ToList().Any(g => g.Equals(text)))
        {
            PluginLog.Debug($"AddonSelectYesNo: Entry is a gimmick");
            addon.Yes();
            return;
        }

        if (P.Config.PartyFinderJoinConfirm && GenericHelpers.TryGetAddonByName<AtkUnitBase>("LookingForGroupDetail", out var _) && lfgPatterns.Any(r => r.IsMatch(text)))
        {
            PluginLog.Debug($"AddonSelectYesNo: Entry is party finder join confirmation");
            addon.Yes();
            return;
        }

        if (P.Config.AutoCollectable && collectablePatterns.Any(text.Contains))
        {
            PluginLog.Debug($"AddonSelectYesNo: Entry is collectable");
            var fish = GenericHelpers.FindRow<Item>(x => x.ItemSearchCategory.RowId == 46 && !x.Singular.IsEmpty && MemoryHelper.ReadSeStringNullTerminated(new nint(addon.Addon->AtkValues[15].String)).GetText().Contains(x.Singular.GetText(), StringComparison.InvariantCultureIgnoreCase));
            if (fish != null)
            {
                PluginLog.Debug($"Detected fish [{fish}] {fish.Value.Name}");
                if (fish.Value.RowId != 0 && int.TryParse(Regex.Match(text, @"\d+").Value, out var value))
                {
                    var collectability = GenericHelpers.FindRow<CollectablesShopItem>(x => x.Item.Value.RowId == fish.Value.RowId);
                    if (collectability != null)
                    {
                        var min = collectability.Value.CollectablesShopRefine.Value.LowCollectability;
                        PluginLog.Debug($"Minimum collectability required is {min}, value detected is {value}");
                        if (value >= min)
                        {
                            PluginLog.Debug($"AddonSelectYesNo: Entry is [{fish}] {fish.Value.Name} with a sufficient collectability of {value}");
                            addon.Yes();
                            return;
                        }
                        else
                        {
                            PluginLog.Debug($"AddonSelectYesNo: Entry is [{fish}] {fish.Value.Name} with an insufficient collectability of {value}");
                            addon.No();
                            return;
                        }
                    }
                    else
                        PluginLog.Debug($"Failed to find matching CollectablesShopItem for [{fish.Value.RowId}] {fish.Value.Name}.");
                }
            }
            else
                PluginLog.Debug($"Failed to match any fish to {MemoryHelper.ReadSeStringNullTerminated(new nint(addon.Addon->AtkValues[15].String)).GetText()}");
        }

        var zoneWarnOnce = true;
        var nodes = P.Config.GetAllNodes().OfType<TextEntryNode>();
        foreach (var node in nodes)
        {
            if (!node.Enabled || string.IsNullOrEmpty(node.Text))
                continue;

            if (!EntryMatchesText(node, text))
                continue;

            if (!ConditionalIsTrue(node, text))
                continue;

            if (node.ZoneRestricted && !string.IsNullOrEmpty(node.ZoneText))
            {
                if (!P.TerritoryNames.TryGetValue(Svc.ClientState.TerritoryType, out var zoneName))
                {
                    if (zoneWarnOnce && !(zoneWarnOnce = false))
                    {
                        PluginLog.Debug("Unable to verify Zone Restricted entry, ZoneID was not set yet");
                        Utils.SEString.PrintPluginMessage($"Unable to verify Zone Restricted entry, change zones to update value");
                    }

                    zoneName = string.Empty;
                }

                if (!string.IsNullOrEmpty(zoneName) && EntryMatchesZoneName(node, zoneName))
                {
                    PluginLog.Debug($"AddonSelectYesNo: Matched on {node.Text} ({node.ZoneText})");
                    if (node.IsYes)
                        addon.Yes();
                    else
                        addon.No();
                    return;
                }
            }
            else if (node.RequiresPlayerConditions && !string.IsNullOrEmpty(node.PlayerConditions))
            {
                var conditions = node.PlayerConditions.Replace(" ", "").Split(',');
                PluginLog.Debug($"conditions: {string.Join(", ", conditions)}");
                if (conditions.All(condition => Enum.TryParse<ConditionFlag>(condition.StartsWith('!') ? condition[1..] : condition, out var flag) && (condition.StartsWith('!') ? !Svc.Condition[flag] : Svc.Condition[flag])))
                {
                    PluginLog.Debug($"AddonSelectYesNo: Matched on {node.Text} and all conditions are true");
                    if (node.IsYes)
                        addon.Yes();
                    else
                        addon.No();
                }
                else
                    PluginLog.Debug($"AddonSelectYesNo: Matched on {node.Text}, but not all conditions were met");
            }
            else
            {
                PluginLog.Debug($"AddonSelectYesNo: Matched on {node.Text}");
                if (node.IsYes)
                    addon.Yes();
                else
                    addon.No();
                return;
            }
        }
    }

    private static bool ConditionalIsTrue(TextEntryNode node, string text)
    {
        if (node.IsConditional)
        {
            PluginLog.Debug("AddonSelectYesNo: Is conditional");
            if (node.ConditionalNumberRegex?.IsMatch(text) ?? false)
            {
                PluginLog.Debug("AddonSelectYesNo: Is conditional matches");
                if (node.ConditionalNumberRegex?.Match(text) is { Success: true, Value: var result } && int.TryParse(result, out var value))
                {
                    PluginLog.Debug($"AddonSelectYesNo: Is conditional - {value}");
                    return node.ComparisonType switch
                    {
                        ComparisonType.LessThan => value < node.ConditionalNumber,
                        ComparisonType.GreaterThan => value > node.ConditionalNumber,
                        ComparisonType.LessThanOrEqual => value <= node.ConditionalNumber,
                        ComparisonType.GreaterThanOrEqual => value >= node.ConditionalNumber,
                        ComparisonType.Equal => value == node.ConditionalNumber,
                        _ => throw new Exception("Uncaught enum"),
                    };
                }
            }

            return false;
        }
        else
        {
            return true;
        }
    }

    private static bool EntryMatchesText(TextEntryNode node, string text)
        => node.IsTextRegex && (node.TextRegex?.IsMatch(text) ?? false) || !node.IsTextRegex && text.Contains(node.Text);

    private static bool EntryMatchesZoneName(TextEntryNode node, string zoneName)
        => node.ZoneIsRegex && (node.ZoneRegex?.IsMatch(zoneName) ?? false) || !node.ZoneIsRegex && zoneName.Contains(node.ZoneText);

    private readonly List<Regex> lfgPatterns =
    [
        new Regex(@"Join .* party\?"),
        new Regex(@".*のパーティに参加します。よろしいですか？"),
        new Regex(@"Der Gruppe von .* beitreten\?"),
        new Regex(@"Rejoindre l'équipe de .*\?")
        // if someone could add the chinese and korean translations that'd be nice
    ];

    private readonly List<string> collectablePatterns =
    [
        "collectability of",
        "収集価値",
        "Sammlerwert",
        "Valeur de collection"
        // if someone could add the chinese and korean translations that'd be nice
    ];
}
