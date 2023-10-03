using System;
using System.Linq;
using System.Runtime.InteropServices;

using ClickLib.Clicks;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

/// <summary>
/// AddonSelectYesNo feature.
/// </summary>
internal class AddonSelectYesNoFeature : OnSetupFeature
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddonSelectYesNoFeature"/> class.
    /// </summary>
    public AddonSelectYesNoFeature()
        : base("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 56 41 57 48 83 EC 40 44 8B F2 0F 29 74 24 ??")
    {
    }

    /// <inheritdoc/>
    protected override string AddonName => "SelectYesNo";

    /// <inheritdoc/>
    protected unsafe override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
    {
        var dataPtr = (AddonSelectYesNoOnSetupData*)data;
        if (dataPtr == null)
            return;

        var text = Service.Plugin.LastSeenDialogText = Service.Plugin.GetSeStringText(dataPtr->TextPtr);
        PluginLog.Debug($"AddonSelectYesNo: text={text}");

        if (Service.Plugin.ForcedYesKeyPressed)
        {
            PluginLog.Debug($"AddonSelectYesNo: Forced yes hotkey pressed");
            this.AddonSelectYesNoExecute(addon, true);
            return;
        }

        var zoneWarnOnce = true;
        var nodes = Service.Configuration.GetAllNodes().OfType<TextEntryNode>();
        foreach (var node in nodes)
        {
            if (!node.Enabled || string.IsNullOrEmpty(node.Text))
                continue;

            if (!this.EntryMatchesText(node, text))
                continue;

            if (node.ZoneRestricted && !string.IsNullOrEmpty(node.ZoneText))
            {
                if (!Service.Plugin.TerritoryNames.TryGetValue(Service.ClientState.TerritoryType, out var zoneName))
                {
                    if (zoneWarnOnce && !(zoneWarnOnce = false))
                    {
                        PluginLog.Debug("Unable to verify Zone Restricted entry, ZoneID was not set yet");
                        Service.Plugin.PrintMessage($"Unable to verify Zone Restricted entry, change zones to update value");
                    }

                    zoneName = string.Empty;
                }

                if (!string.IsNullOrEmpty(zoneName) && this.EntryMatchesZoneName(node, zoneName))
                {
                    PluginLog.Debug($"AddonSelectYesNo: Matched on {node.Text} ({node.ZoneText})");
                    this.AddonSelectYesNoExecute(addon, node.IsYes);
                    return;
                }
            }
            else
            {
                PluginLog.Debug($"AddonSelectYesNo: Matched on {node.Text}");
                this.AddonSelectYesNoExecute(addon, node.IsYes);
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
                PluginLog.Debug("AddonSelectYesNo: Enabling yes button");
                var flagsPtr = (ushort*)&yesButton->AtkComponentBase.OwnerNode->AtkResNode.NodeFlags;
                *flagsPtr ^= 1 << 5;
            }

            PluginLog.Debug("AddonSelectYesNo: Selecting yes");
            ClickSelectYesNo.Using(addon).Yes();
        }
        else
        {
            PluginLog.Debug("AddonSelectYesNo: Selecting no");
            ClickSelectYesNo.Using(addon).No();
        }
    }

    private bool EntryMatchesText(TextEntryNode node, string text)
    {
        return (node.IsTextRegex && (node.TextRegex?.IsMatch(text) ?? false)) ||
              (!node.IsTextRegex && text.Contains(node.Text));
    }

    private bool EntryMatchesZoneName(TextEntryNode node, string zoneName)
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
