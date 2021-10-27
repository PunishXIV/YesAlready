using System;

using ClickLib.Clicks;
using FFXIVClientStructs.FFXIV.Client.UI;
using YesAlready.BaseFeatures;

namespace YesAlready.Features
{
    /// <summary>
    /// AddonJournalResult feature.
    /// </summary>
    internal class AddonJournalResultFeature : OnSetupFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddonJournalResultFeature"/> class.
        /// </summary>
        public AddonJournalResultFeature()
            : base(Service.Address.AddonJournalResultOnSetupAddress)
        {
        }

        /// <inheritdoc/>
        protected override string AddonName => "JournalResult";

        /// <inheritdoc/>
        protected unsafe override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
        {
            if (!Service.Configuration.JournalResultCompleteEnabled)
                return;

            var addonPtr = (AddonJournalResult*)addon;
            var completeButton = addonPtr->CompleteButton;
            if (!addonPtr->CompleteButton->IsEnabled)
                return;

            ClickJournalResult.Using(addon).Complete();
        }
    }
}
