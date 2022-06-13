using System;

using ClickLib.Clicks;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

/// <summary>
/// AddonGrandCompanySupplyReward feature.
/// </summary>
internal class AddonGrandCompanySupplyRewardFeature : OnSetupFeature
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddonGrandCompanySupplyRewardFeature"/> class.
    /// </summary>
    public AddonGrandCompanySupplyRewardFeature()
        : base("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 54 41 55 41 56 41 57 48 83 EC 30 BA ?? ?? ?? ?? 4D 8B E8 4C 8B F9")
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
