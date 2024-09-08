using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class SalvageResult : BaseFeature
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
        new AddonMaster.SalvageResult(addonInfo.Base()).Close();
    }

    protected static unsafe void AddonUpdate(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active || !P.Config.DesynthesisResults || !GenericHelpers.IsAddonReady(addonInfo.Base())) return;
        if (GenericHelpers.TryGetAddonMaster<AddonMaster.SalvageAutoDialog>(out var am))
        {
            if (am.DesynthesisInactive)
                am.EndDesynthesis();
        }
    }
}
