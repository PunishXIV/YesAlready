using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons;
using ECommons.UIHelpers.AddonMasterImplementations;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class AddonSalvageResult : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "SalvageResult", AddonSetup);
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, "SalvageAutoDialog", AddonUpdate);
    }

    public override void Disable()
    {
        base.Disable();
        Svc.AddonLifecycle.UnregisterListener(AddonSetup);
        Svc.AddonLifecycle.UnregisterListener(AddonUpdate);
    }

    protected static unsafe void AddonSetup(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active || !P.Config.DesynthesisResults || !GenericHelpers.IsAddonReady(addonInfo.Base())) return;
        var addon = new AddonMaster.SalvageResult(addonInfo.Base());
        addon.Close();
    }

    protected static unsafe void AddonUpdate(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active || !P.Config.DesynthesisResults || !GenericHelpers.IsAddonReady(addonInfo.Base())) return;
        var addon = new AddonMaster.SalvageAutoDialog(addonInfo.Base());
        if (addon.DesynthesisInactive)
            addon.EndDesynthesis();
    }
}
