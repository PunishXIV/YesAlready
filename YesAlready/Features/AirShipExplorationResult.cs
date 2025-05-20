namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class AirShipExplorationResult : AddonFeature
{
    protected override bool IsEnabled() => C.AirShipExplorationResultFinalize || C.AirShipExplorationResultRedeploy;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk)
    {
        var addon = new AddonMaster.AirShipExplorationResult(atk);

        if (C.AirShipExplorationResultFinalize)
            addon.FinalizeReport();

        if (C.AirShipExplorationResultRedeploy)
            addon.Redeploy();
    }
}
