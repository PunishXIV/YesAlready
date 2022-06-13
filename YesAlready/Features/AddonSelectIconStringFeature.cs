using System;

using ClickLib.Clicks;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

/// <summary>
/// AddonSelectString feature.
/// </summary>
internal class AddonSelectIconStringFeature : OnSetupSelectListFeature
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddonSelectIconStringFeature"/> class.
    /// </summary>
    public AddonSelectIconStringFeature()
        : base("40 53 56 57 41 54 41 57 48 83 EC 30 4D 8B F8 44 8B E2 48 8B F1 E8 ?? ?? ?? ??")
    {
    }

    /// <inheritdoc/>
    protected override string AddonName => "SelectIconString";

    /// <inheritdoc/>
    protected unsafe override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
    {
        var addonPtr = (AddonSelectIconString*)addon;
        var popupMenu = &addonPtr->PopupMenu.PopupMenu;

        this.SetupOnItemSelectedHook(popupMenu);
        this.CompareNodesToEntryTexts(addon, popupMenu);
    }

    /// <inheritdoc/>
    protected override void SelectItemExecute(IntPtr addon, int index)
    {
        PluginLog.Debug($"AddonSelectIconString: Selecting {index}");
        ClickSelectIconString.Using(addon).SelectItem((ushort)index);
    }
}
