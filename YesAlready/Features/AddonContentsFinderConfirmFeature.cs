using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons.UIHelpers.AddonMasterImplementations;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class AddonContentsFinderConfirmFeature : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "ContentsFinderConfirm", AddonSetup);
    }

    public override void Disable()
    {
        base.Disable();
        Svc.AddonLifecycle.UnregisterListener(AddonSetup);
    }

    protected static unsafe void AddonSetup(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active || !P.Config.ContentsFinderConfirmEnabled) return;

        var addon = new AddonMaster.ContentsFinderConfirm(addonInfo.Base());
        addon.Commence();

        if (P.Config.ContentsFinderOneTimeConfirmEnabled)
        {
            P.Config.ContentsFinderConfirmEnabled = false;
            P.Config.ContentsFinderOneTimeConfirmEnabled = false;
            P.Config.Save();
        }
    }
}
