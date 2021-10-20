using System;

using ClickLib.Clicks;
using YesAlready.BaseFeatures;

namespace YesAlready.Features
{
    /// <summary>
    /// AddonMaterializeDialog feature.
    /// </summary>
    internal class AddonMaterializeDialogFeature : OnSetupFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddonMaterializeDialogFeature"/> class.
        /// </summary>
        public AddonMaterializeDialogFeature()
            : base(Service.Address.AddonMaterializeDialongOnSetupAddress)
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
}
