namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class GrandCompanySupplyReward : AddonFeature
{
    protected override bool IsEnabled() => P.Config.GrandCompanySupplyReward;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk) => new AddonMaster.GrandCompanySupplyReward(atk).Deliver();
}
