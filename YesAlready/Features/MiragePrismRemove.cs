namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class MiragePrismRemove : AddonFeature
{
    protected override bool IsEnabled() => P.Config.MiragePrismRemoveDispel;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk) => new AddonMaster.MiragePrismRemove(atk).Dispel();
}
