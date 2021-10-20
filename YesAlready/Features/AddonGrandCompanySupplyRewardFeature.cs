using System;

using ClickLib.Clicks;
using YesAlready.BaseFeatures;

namespace YesAlready.Features
{
    /// <summary>
    /// AddonGrandCompanySupplyReward feature.
    /// </summary>
    internal class AddonGrandCompanySupplyRewardFeature : OnSetupFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddonGrandCompanySupplyRewardFeature"/> class.
        /// </summary>
        public AddonGrandCompanySupplyRewardFeature()
            : base(Service.Address.AddonGrandCompanySupplyRewardOnSetupAddress)
        {
        }

        /// <inheritdoc/>
        protected override string AddonName => "GrandCompanySupplyReward";

        /// <inheritdoc/>
        protected unsafe override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
        {
            if (!Service.Configuration.GrandCompanySupplyReward)
                return;

            ClickGrandCompanySupplyReward.Using(addon).Deliver();
        }
    }
}
