using System;

using ClickLib.Clicks;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

/// <summary>
/// AddonContentsFinderConfirm feature.
/// </summary>
internal class AddonContentsFinderConfirmFeature : OnSetupFeature
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddonContentsFinderConfirmFeature"/> class.
    /// </summary>
    public AddonContentsFinderConfirmFeature()
        : base("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 56 41 57 48 83 EC 30 44 8B F2 49 8B E8 BA ?? ?? ?? ?? 48 8B D9")
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
