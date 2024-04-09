using ClickLib.Clicks;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class AddonSelectYesNoFeature : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "SelectYesno", AddonSetup);
    }

    public override void Disable()
    {
        base.Disable();
        AddonLifecycle.UnregisterListener(AddonSetup);
    }

    protected unsafe void AddonSetup(AddonEvent eventType, AddonArgs addonInfo)
    {
        var addon = (AtkUnitBase*)addonInfo.Addon;

        if (!P.Active)
            return;

        var dataPtr = (AddonSelectYesNoOnSetupData*)addon;
        if (dataPtr == null)
            return;

        var text = P.LastSeenDialogText = Utils.SEString.GetSeStringText(new nint(addon->AtkValues[0].String));
        Svc.Log.Debug($"AddonSelectYesNo: text={text}");

        if (P.ForcedYesKeyPressed)
        {
            Svc.Log.Debug($"AddonSelectYesNo: Forced yes hotkey pressed");
            AddonSelectYesNoExecute((nint)addon, true);
            return;
        }

        if (P.Config.PartyFinderJoinConfirm && GenericHelpers.TryGetAddonByName<AtkUnitBase>("LookingForGroupDetail", out var _) && lfgPatterns.Any(r => r.IsMatch(text)))
        {
            Svc.Log.Debug($"AddonSelectYesNo: Entry is party finder join confirmation");
            AddonSelectYesNoExecute((nint)addon, true);
            return;
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
                        Svc.Log.Debug("Unable to verify Zone Restricted entry, ZoneID was not set yet");
                        Utils.SEString.PrintPluginMessage($"Unable to verify Zone Restricted entry, change zones to update value");
                    }

                    zoneName = string.Empty;
                }

                if (!string.IsNullOrEmpty(zoneName) && EntryMatchesZoneName(node, zoneName))
                {
                    Svc.Log.Debug($"AddonSelectYesNo: Matched on {node.Text} ({node.ZoneText})");
                    AddonSelectYesNoExecute((nint)addon, node.IsYes);
                    return;
                }
            }
            else
            {
                Svc.Log.Debug($"AddonSelectYesNo: Matched on {node.Text}");
                AddonSelectYesNoExecute((nint)addon, node.IsYes);
                return;
            }
        }
    }

    private static bool ConditionalIsTrue(TextEntryNode node, string text)
    {
        if (node.IsConditional)
        {
            Svc.Log.Debug("AddonSelectYesNo: Is conditional");
            if (node.ConditionalNumberRegex?.IsMatch(text) ?? false)
            {
                Svc.Log.Debug("AddonSelectYesNo: Is conditional matches");
                var result = node.ConditionalNumberRegex?.Match(text);
                if (result.Success && int.TryParse(result.Value, out int value))
                {
                    Svc.Log.Debug($"AddonSelectYesNo: Is conditional - {value}");
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

    private unsafe void AddonSelectYesNoExecute(IntPtr addon, bool yes)
    {
        if (yes)
        {
            var addonPtr = (AddonSelectYesno*)addon;
            var yesButton = addonPtr->YesButton;
            if (yesButton != null && !yesButton->IsEnabled)
            {
                Svc.Log.Debug("AddonSelectYesNo: Enabling yes button");
                var flagsPtr = (ushort*)&yesButton->AtkComponentBase.OwnerNode->AtkResNode.NodeFlags;
                *flagsPtr ^= 1 << 5;
            }

            Svc.Log.Debug("AddonSelectYesNo: Selecting yes");
            ClickSelectYesNo.Using(addon).Yes();
        }
        else
        {
            Svc.Log.Debug("AddonSelectYesNo: Selecting no");
            ClickSelectYesNo.Using(addon).No();
        }
    }

    private static bool EntryMatchesText(TextEntryNode node, string text)
    {
        return (node.IsTextRegex && (node.TextRegex?.IsMatch(text) ?? false)) ||
              (!node.IsTextRegex && text.Contains(node.Text));
    }

    private static bool EntryMatchesZoneName(TextEntryNode node, string zoneName)
    {
        return (node.ZoneIsRegex && (node.ZoneRegex?.IsMatch(zoneName) ?? false)) ||
              (!node.ZoneIsRegex && zoneName.Contains(node.ZoneText));
    }

    private readonly List<Regex> lfgPatterns =
    [
        new Regex(@"Join .* party\?"),
        new Regex(@".*のパーティに参加します。よろしいですか？"),
        new Regex(@"Der Gruppe von .* beitreten\?"),
        new Regex(@"Rejoindre l'équipe de .*\?")
    ];

    [StructLayout(LayoutKind.Explicit, Size = 0x10)]
    private struct AddonSelectYesNoOnSetupData
    {
        [FieldOffset(0x8)]
        public IntPtr TextPtr;
    }
}
