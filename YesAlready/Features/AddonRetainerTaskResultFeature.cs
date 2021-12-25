using System;

using ClickLib.Clicks;
using FFXIVClientStructs.FFXIV.Client.UI;
using YesAlready.BaseFeatures;

namespace YesAlready.Features
{
    /// <summary>
    /// AddonRetainerTaskResult feature.
    /// </summary>
    internal class AddonRetainerTaskResultFeature : OnSetupFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddonRetainerTaskResultFeature"/> class.
        /// </summary>
        public AddonRetainerTaskResultFeature()
            : base(Service.Address.AddonRetainerTaskResultOnSetupAddress)
        {
        }

        /// <inheritdoc/>
        protected override string AddonName => "RetainerTaskResult";

        /// <inheritdoc/>
        protected unsafe override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
        {
            if (!Service.Configuration.RetainerTaskResultEnabled)
                return;

            var addonPtr = (AddonRetainerTaskResult*)addon;
            var buttonText = addonPtr->ReassignButton->ButtonTextNode->NodeText.ToString();
            if (buttonText == "Recall" ||
                buttonText == "中断する" ||
                buttonText == "Zurückrufen" ||
                buttonText == "Interrompre")
                return;

            ClickRetainerTaskResult.Using(addon).Reassign();
        }
    }
}
