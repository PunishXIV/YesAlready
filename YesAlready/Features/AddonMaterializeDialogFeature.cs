using System;

using ClickLib.Clicks;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

/// <summary>
/// AddonMaterializeDialog feature.
/// </summary>
internal class AddonMaterializeDialogFeature : OnSetupFeature
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddonMaterializeDialogFeature"/> class.
    /// </summary>
    public AddonMaterializeDialogFeature()
        : base("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 41 56 48 83 EC 50 44 8B F2 49 8B E8 BA ?? ?? ?? ??")
    {
    }

    /// <inheritdoc/>
    protected override string AddonName => "MaterializeDialog";

    /// <inheritdoc/>
    protected unsafe override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
    {
        if (!Service.Configuration.MaterializeDialogEnabled)
            return;

        ClickMaterializeDialog.Using(addon).Materialize();
    }
}
