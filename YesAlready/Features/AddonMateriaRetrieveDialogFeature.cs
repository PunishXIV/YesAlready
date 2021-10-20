using System;

using ClickLib.Clicks;
using YesAlready.BaseFeatures;

namespace YesAlready.Features
{
    /// <summary>
    /// AddonMateriaRetrieveDialog feature.
    /// </summary>
    internal class AddonMateriaRetrieveDialogFeature : OnSetupFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddonMateriaRetrieveDialogFeature"/> class.
        /// </summary>
        public AddonMateriaRetrieveDialogFeature()
            : base(Service.Address.AddonMateriaRetrieveDialongOnSetupAddress)
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
}
