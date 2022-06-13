using System;

using ClickLib.Clicks;
using FFXIVClientStructs.FFXIV.Client.UI;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

/// <summary>
/// AddonRetainerTaskResult feature.
/// </summary>
internal class AddonRetainerTaskResultFeature : OnSetupFeature
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddonRetainerTaskResultFeature"/> class.
    /// </summary>
    public AddonRetainerTaskResultFeature()
        : base("48 89 5C 24 ?? 55 56 57 48 83 EC 40 8B F2 49 8B F8 BA ?? ?? ?? ?? 48 8B D9 E8 ?? ?? ?? ??")
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
