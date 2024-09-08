using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class AirShipExplorationResult : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "AirShipExplorationResult", AddonSetup);
    }

    public override void Disable()
    {
        base.Disable();
        Svc.AddonLifecycle.UnregisterListener(AddonSetup);
    }

    protected static unsafe void AddonSetup(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active) return;
        if (P.Config.AirShipExplorationResultFinalize)
            new AddonMaster.AirShipExplorationResult(addonInfo.Base()).FinalizeReport();
        if (P.Config.AirShipExplorationResultRedeploy)
            new AddonMaster.AirShipExplorationResult(addonInfo.Base()).Redeploy();
    }
}
