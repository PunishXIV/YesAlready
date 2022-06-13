using System;

using ClickLib.Clicks;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

/// <summary>
/// AddonMateriaRetrieveDialog feature.
/// </summary>
internal class AddonMateriaRetrieveDialogFeature : OnSetupFeature
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddonMateriaRetrieveDialogFeature"/> class.
    /// </summary>
    public AddonMateriaRetrieveDialogFeature()
        : base("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 30 8B FA 49 8B D8 BA ?? ?? ?? ?? 48 8B F1 E8 ?? ?? ?? ?? 48 8B C8")
    {
    }

    /// <inheritdoc/>
    protected override string AddonName => "MateriaRetrieveDialog";

    /// <inheritdoc/>
    protected unsafe override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
    {
        if (!Service.Configuration.MateriaRetrieveDialogEnabled)
            return;

        ClickMateriaRetrieveDialog.Using(addon).Begin();
    }
}
