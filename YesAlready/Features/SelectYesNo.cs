using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Memory;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
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
        var text = GetTextLegacy(atk);
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

        if (C.GimmickYesNo && Svc.Data.GetExcelSheet<GimmickYesNo>().Where(x => !x.Message.IsEmpty).Select(x => x.Message).ToList().Any(g => g.EqualsIgnoreSpecial(text)))
        {
            Log($"Entry is a gimmick");
            return new TextEntryNode { IsYes = true };
        }

        if (C.PartyFinderJoinConfirm && IsPartyFinderJoinConfirm(atk))
        {
            Log($"Entry is party finder join confirmation");
            return new TextEntryNode { IsYes = true };
        }

        if (C.AutoCollectable && collectablePatterns.Any(text.Contains))
        {
            Log($"Entry is collectable");
            var addon = (AddonSelectYesno*)atk;
            if (!addon->CollectibleAtkValuesAvailable)
            {
                Log("Collectible AtkValues not available");
            }
            else
            {
                if (GenericHelpers.GetRow<Item>(ItemUtil.GetBaseId(addon->CollectibleTypedAtkValues->ItemId.UInt).ItemId) is { } item)
                {
                    Log($"Detected item [{item}] {item.Name}");
                    if (int.TryParse(Regex.Match(text, @"\d+").Value, out var value))
                    {
                        if (GenericHelpers.FindSubrow<CollectablesShopItem>(x => x.Item.Value.RowId == item.RowId) is { } collectability)
                        {
                            var min = collectability.CollectablesShopRefine.Value.LowCollectability;
                            Log($"Minimum collectability required is {min}, value detected is {value}");
                            if (value >= min)
                            {
                                Log($"Entry is [{item}] {item.Name} with a sufficient collectability of {value}");
                                return new TextEntryNode { IsYes = true };
                            }

                            Log($"Entry is [{item}] {item.Name} with an insufficient collectability of {value}");
                            return new TextEntryNode { IsYes = false };
                        }

                        if (item.AetherialReduce > 0) // aethersand fish aren't turned in for scrips so collectability doesn't matter
                        {
                            Log($"Entry is [#{item.RowId}] {item.Name} and probably an aethersand fish. Skipping collectability check.");
                            return new TextEntryNode { IsYes = true };
                        }

                        if (GenericHelpers.TryGetRow<WKSItemInfo>(item.AdditionalData.RowId, out var wksItem)) // stellar fish are scored based on collective collectability so individual doesn't matter
                        {
                            Log($"Entry is [#{item.RowId}] {item.Name} for {wksItem.WKSItemSubCategory.ValueNullable?.Name ?? "null"}. Skipping collectability check.");
                            return new TextEntryNode { IsYes = true };
                        }

                        Log($"Failed to find matching CollectablesShopItem for [{item.RowId}] {item.Name}. Not an aethersand fish or a CE fish. Ping the dev or create a git issue if you found this message erroneously.");
                    }
                }
                else
                    Log($"Failed to match any collectable to item id {ItemUtil.GetBaseId(addon->CollectibleTypedAtkValues->ItemId.UInt).ItemId}");
            }
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
        var addon = (AddonSelectYesno*)atk;
        if (node.IsYes)
            addon->YesButton->Click();
        else
            addon->NoButton->Click();
    }

    private static unsafe bool IsPartyFinderJoinConfirm(AtkUnitBase* selectYesno)
    {
        var agent = AgentLookingForGroup.Instance();
        if (agent == null || !agent->IsAgentActive())
            return false;

        var joinConfirmAddonId = agent->JoinConfirmAddonId;
        return joinConfirmAddonId != 0 && selectYesno->Id == joinConfirmAddonId;
    }

    private static unsafe string GetTextLegacy(AtkUnitBase* atk)
    {
        var addon = (AddonSelectYesno*)atk;
        if (addon->AtkValues == null || addon->AtkValuesCount == 0 || !addon->StandardTypedAtkValues->PromptText.String.HasValue)
            return string.Empty;

        var se = MemoryHelper.ReadSeStringNullTerminated((nint)addon->StandardTypedAtkValues->PromptText.String.Value);
        return string.Join(string.Empty, se.Payloads.OfType<TextPayload>().Select(t => t.Text)).Replace('\n', ' ').Trim();
    }

    private readonly List<string> collectablePatterns =
    [
        "collectability of",
        "収集価値",
        "Sammlerwert",
        "Valeur de collection"
        // if someone could add the chinese and korean translations that'd be nice
    ];
}
