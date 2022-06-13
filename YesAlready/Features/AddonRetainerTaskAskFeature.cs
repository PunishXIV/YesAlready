using System;

using ClickLib.Clicks;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

/// <summary>
/// AddonRetainerTaskAsk feature.
/// </summary>
internal class AddonRetainerTaskAskFeature : OnSetupFeature
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddonRetainerTaskAskFeature"/> class.
    /// </summary>
    public AddonRetainerTaskAskFeature()
        : base("40 53 48 83 EC 30 48 8B D9 83 FA 03 7C 53 49 8B C8 E8 ?? ?? ?? ??")
    {
    }

    /// <inheritdoc/>
    protected override string AddonName => "RetainerTaskAsk";

    /// <inheritdoc/>
    protected unsafe override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
    {
        if (!Service.Configuration.RetainerTaskAskEnabled)
            return;

        ClickRetainerTaskAsk.Using(addon).Assign();
    }
}
