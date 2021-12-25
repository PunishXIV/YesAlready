using System;

using ClickLib.Clicks;
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

            ClickRetainerTaskResult.Using(addon).Reassign();
        }
    }
}
