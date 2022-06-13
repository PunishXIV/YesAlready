using System;

using ClickLib.Clicks;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

/// <summary>
/// AddonItemInspectionResult feature.
/// </summary>
internal class AddonItemInspectionResultFeature : OnSetupFeature
{
    private int itemInspectionCount = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddonItemInspectionResultFeature"/> class.
    /// </summary>
    public AddonItemInspectionResultFeature()
        : base("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 30 8B F2 49 8B F8 BA ?? ?? ?? ?? 48 8B D9 E8 ?? ?? ?? ?? 48 8B C8 E8 ?? ?? ?? ?? 48 8B D0")
    {
    }

    /// <inheritdoc/>
    protected override string AddonName => "ItemInspectionResult";

    /// <inheritdoc/>
    protected unsafe override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
    {
        if (!Service.Configuration.ItemInspectionResultEnabled)
            return;

        var addonPtr = (AddonItemInspectionResult*)addon;
        if (addonPtr->AtkUnitBase.UldManager.NodeListCount < 64)
            return;

        var nameNode = (AtkTextNode*)addonPtr->AtkUnitBase.UldManager.NodeList[64];
        var descNode = (AtkTextNode*)addonPtr->AtkUnitBase.UldManager.NodeList[55];
        if (!nameNode->AtkResNode.IsVisible || !descNode->AtkResNode.IsVisible)
            return;

        var nameText = Service.Plugin.GetSeString(nameNode->NodeText.StringPtr);
        var descText = Service.Plugin.GetSeStringText(descNode->NodeText.StringPtr);
        // This is hackish, but works well enough (for now).
        // Languages that dont contain the magic character will need special handling.
        if (descText.Contains("※") || descText.Contains("liées à Garde-la-Reine"))
        {
            nameText.Payloads.Insert(0, new TextPayload("Received: "));
            Service.Plugin.PrintMessage(nameText);
        }

        this.itemInspectionCount++;
        var rateLimiter = Service.Configuration.ItemInspectionResultRateLimiter;
        if (rateLimiter != 0 && this.itemInspectionCount % rateLimiter == 0)
        {
            this.itemInspectionCount = 0;
            Service.Plugin.PrintMessage("Rate limited, pausing item inspection loop.");
            return;
        }

        ClickItemInspectionResult.Using(addon).Next();
    }
}
