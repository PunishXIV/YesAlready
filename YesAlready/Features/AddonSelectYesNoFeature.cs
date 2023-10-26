using System;
using System.Linq;
using System.Runtime.InteropServices;

using ClickLib.Clicks;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Memory;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
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

        if (!P.Config.Enabled)
            return;

        var dataPtr = (AddonSelectYesNoOnSetupData*)addon;
        if (dataPtr == null)
            return;

        //var text = P.LastSeenDialogText = Utils.SEString.GetSeStringText(dataPtr->TextPtr);
        var text = P.LastSeenDialogText = MemoryHelper.ReadSeStringNullTerminated(new nint(addon->AtkValues[0].String)).ToString();
        Svc.Log.Debug($"AddonSelectYesNo: text={text}");

        if (P.ForcedYesKeyPressed)
        {
            Svc.Log.Debug($"AddonSelectYesNo: Forced yes hotkey pressed");
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

    [StructLayout(LayoutKind.Explicit, Size = 0x10)]
    private struct AddonSelectYesNoOnSetupData
    {
        [FieldOffset(0x8)]
        public IntPtr TextPtr;
    }
}
