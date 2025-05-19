namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class AirShipExplorationResult : AddonFeature
{
    protected override bool IsEnabled() => P.Config.AirShipExplorationResultFinalize || P.Config.AirShipExplorationResultRedeploy;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk)
    {
        var addon = new AddonMaster.AirShipExplorationResult(atk);

        if (P.Config.AirShipExplorationResultFinalize)
            addon.FinalizeReport();

        if (P.Config.AirShipExplorationResultRedeploy)
            addon.Redeploy();
    }
}
