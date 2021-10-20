using System;

using ClickLib.Clicks;
using FFXIVClientStructs.FFXIV.Client.UI;
using YesAlready.BaseFeatures;

namespace YesAlready.Features
{
    /// <summary>
    /// AddonShopCardDialog feature.
    /// </summary>
    internal class AddonShopCardDialogFeature : OnSetupFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddonShopCardDialogFeature"/> class.
        /// </summary>
        public AddonShopCardDialogFeature()
            : base(Service.Address.AddonShopCardDialogOnSetupAddress)
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
}
