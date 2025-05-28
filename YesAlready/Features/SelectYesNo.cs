using Dalamud.Memory;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class SelectYesno : TextMatchingFeature
{
    protected override unsafe string GetSetLastSeenText(AtkUnitBase* atk)
    {
        var text = new AddonMaster.SelectYesno(atk).TextLegacy;
        Service.Watcher.LastSeenDialogText = text;
        return text;
    }

    protected override unsafe object? ShouldProceed(string text, AtkUnitBase* atk)
    {
        if (Service.Watcher.ForcedYesKeyPressed)
        {
            Log($"Forced yes hotkey pressed");
            return new TextEntryNode { IsYes = true };
        }

        if (C.GimmickYesNo && Svc.Data.GetExcelSheet<GimmickYesNo>().Where(x => !x.Unknown0.IsEmpty).Select(x => x.Unknown0).ToList().Any(g => g.Equals(text)))
        {
            Log($"Entry is a gimmick");
            return new TextEntryNode { IsYes = true };
        }

        if (C.PartyFinderJoinConfirm && GenericHelpers.TryGetAddonByName<AtkUnitBase>("LookingForGroupDetail", out var _) && lfgPatterns.Any(r => r.IsMatch(text)))
        {
            Log($"Entry is party finder join confirmation");
            return new TextEntryNode { IsYes = true };
        }

        if (C.AutoCollectable && collectablePatterns.Any(text.Contains))
        {
            Log($"Entry is collectable");
            var fish = GenericHelpers.FindRow<Item>(x => x.ItemSearchCategory.RowId == 46 && !x.Singular.IsEmpty && MemoryHelper.ReadSeStringNullTerminated(new nint(atk->AtkValues[15].String)).GetText().Contains(x.Singular.GetText(), StringComparison.InvariantCultureIgnoreCase));
            if (fish != null)
            {
                Log($"Detected fish [{fish}] {fish.Value.Name}");
                if (fish.Value.RowId != 0 && int.TryParse(Regex.Match(text, @"\d+").Value, out var value))
                {
                    if (GenericHelpers.FindRow<CollectablesShopItem>(x => x.Item.Value.RowId == fish.Value.RowId) is { } collectability)
                    {
                        var min = collectability.CollectablesShopRefine.Value.LowCollectability;
                        Log($"Minimum collectability required is {min}, value detected is {value}");
                        if (value >= min)
                        {
                            Log($"Entry is [{fish}] {fish.Value.Name} with a sufficient collectability of {value}");
                            return new TextEntryNode { IsYes = true };
                        }
                        else
                        {
                            Log($"Entry is [{fish}] {fish.Value.Name} with an insufficient collectability of {value}");
                            return new TextEntryNode { IsYes = false };
                        }
                    }
                    else
                        Log($"Failed to find matching CollectablesShopItem for [{fish.Value.RowId}] {fish.Value.Name}.");
                }
            }
            else
                Log($"Failed to match any fish to {MemoryHelper.ReadSeStringNullTerminated(new nint(atk->AtkValues[15].String)).GetText()}");
        }

        var nodes = C.GetAllNodes().OfType<TextEntryNode>();
        foreach (var node in nodes)
        {
            if (!node.Enabled || string.IsNullOrEmpty(node.Text))
                continue;

            if (!CheckRestrictions(node))
                continue;

            if (EntryMatchesText(node.Text, text, node.IsTextRegex))
                return node;
        }

        return null;
    }

    protected override unsafe void Proceed(AtkUnitBase* atk, object? matchingNode)
    {
        if (matchingNode is not TextEntryNode node) return;
        if (node.IsYes)
            new AddonMaster.SelectYesno(atk).Yes();
        else
            new AddonMaster.SelectYesno(atk).No();
    }

    private static readonly List<Regex> lfgPatterns =
    [
        new(@"^Do you wish to join the party\?$"),
        new(@"^Do you wish to join the alliance\?$"),
        new(@"^Do you wish to join the cross-world party\?$"),
        new(@"^Do you wish to join the cross-world alliance\?$")
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
