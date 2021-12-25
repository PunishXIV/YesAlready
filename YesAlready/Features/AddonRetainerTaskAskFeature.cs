using System;

using ClickLib.Clicks;
using FFXIVClientStructs.FFXIV.Client.UI;
using YesAlready.BaseFeatures;

namespace YesAlready.Features
{
    /// <summary>
    /// AddonRetainerTaskAsk feature.
    /// </summary>
    internal class AddonRetainerTaskAskFeature : OnSetupFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddonRetainerTaskAskFeature"/> class.
        /// </summary>
        public AddonRetainerTaskAskFeature()
            : base(Service.Address.AddonRetainerTaskAskOnSetupAddress)
        {
        }

        /// <inheritdoc/>
        protected override string AddonName => "RetainerTaskAsk";

        /// <inheritdoc/>
        protected unsafe override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
        {
            if (!Service.Configuration.RetainerTaskAskEnabled)
                return;

            var addonPtr = (AddonRetainerTaskAsk*)addon;
            if (!addonPtr->AssignButton->IsEnabled)
                return;

            ClickRetainerTaskAsk.Using(addon).Assign();
        }
    }
}
