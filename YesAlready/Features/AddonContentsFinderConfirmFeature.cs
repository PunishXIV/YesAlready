using System;

using ClickLib.Clicks;
using YesAlready.BaseFeatures;

namespace YesAlready.Features
{
    /// <summary>
    /// AddonContentsFinderConfirm feature.
    /// </summary>
    internal class AddonContentsFinderConfirmFeature : OnSetupFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddonContentsFinderConfirmFeature"/> class.
        /// </summary>
        public AddonContentsFinderConfirmFeature()
            : base(Service.Address.AddonContentsFinderConfirmOnSetupAddress)
        {
        }

        /// <inheritdoc/>
        protected override string AddonName => "ContentsFinderConfirm";

        /// <inheritdoc/>
        protected unsafe override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
        {
            if (!Service.Configuration.ContentsFinderConfirmEnabled)
                return;

            ClickContentsFinderConfirm.Using(addon).Commence();

            if (Service.Configuration.ContentsFinderOneTimeConfirmEnabled)
            {
                Service.Configuration.ContentsFinderConfirmEnabled = false;
                Service.Configuration.ContentsFinderOneTimeConfirmEnabled = false;
                Service.Configuration.Save();
            }
        }
    }
}
