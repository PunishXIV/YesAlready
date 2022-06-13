using System;

using ClickLib.Clicks;
using FFXIVClientStructs.FFXIV.Client.UI;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

/// <summary>
/// AddonShopCardDialog feature.
/// </summary>
internal class AddonShopCardDialogFeature : OnSetupFeature
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddonShopCardDialogFeature"/> class.
    /// </summary>
    public AddonShopCardDialogFeature()
        : base("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 41 54 41 56 41 57 48 83 EC 50 48 8B F9 49 8B F0")
    {
    }

    /// <inheritdoc/>
    protected override string AddonName => "ShopCardDialog";

    /// <inheritdoc/>
    protected unsafe override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
    {
        if (!Service.Configuration.ShopCardDialog)
            return;

        var addonPtr = (AddonShopCardDialog*)addon;
        if (addonPtr->CardQuantityInput != null)
            addonPtr->CardQuantityInput->SetValue(addonPtr->CardQuantityInput->Data.Max);

        ClickShopCardDialog.Using(addon).Sell();
    }
}
