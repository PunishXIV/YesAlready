using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons.UIHelpers.AddonMasterImplementations;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class AddonGrandCompanySupplyRewardFeature : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "GrandCompanySupplyReward", AddonSetup);
    }

    public override void Disable()
    {
        base.Disable();
        Svc.AddonLifecycle.UnregisterListener(AddonSetup);
    }

    protected static unsafe void AddonSetup(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active || !P.Config.GrandCompanySupplyReward) return;
        var addon = new AddonMaster.GrandCompanySupplyReward(addonInfo.Base());
        addon.Deliver();
    }
}
