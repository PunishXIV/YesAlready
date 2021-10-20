using System;

using ClickLib.Clicks;
using FFXIVClientStructs.FFXIV.Client.UI;
using YesAlready.BaseFeatures;

namespace YesAlready.Features
{
    /// <summary>
    /// AddonSalvageDialog feature.
    /// </summary>
    internal class AddonSalvageDialogFeature : OnSetupFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddonSalvageDialogFeature"/> class.
        /// </summary>
        public AddonSalvageDialogFeature()
            : base(Service.Address.AddonSalvageDialongOnSetupAddress)
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
}
