using System;

using ClickLib.Clicks;
using FFXIVClientStructs.FFXIV.Client.UI;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

/// <summary>
/// AddonSalvageDialog feature.
/// </summary>
internal class AddonSalvageDialogFeature : OnSetupFeature
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddonSalvageDialogFeature"/> class.
    /// </summary>
    public AddonSalvageDialogFeature()
        : base("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 41 56 48 83 EC 30 44 8B F2 49 8B E8")
    {
    }

    /// <inheritdoc/>
    protected override string AddonName => "SalvageDialog";

    /// <inheritdoc/>
    protected unsafe override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
    {
        if (Service.Configuration.DesynthBulkDialogEnabled)
        {
            ((AddonSalvageDialog*)addon)->BulkDesynthEnabled = true;
        }

        if (Service.Configuration.DesynthDialogEnabled)
        {
            var clickAddon = ClickSalvageDialog.Using(addon);
            clickAddon.CheckBox();
            clickAddon.Desynthesize();
        }
    }
}
