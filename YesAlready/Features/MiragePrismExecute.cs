namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class MiragePrismExecute : AddonFeature
{
    protected override bool IsEnabled() => P.Config.MiragePrismExecuteCast;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk) => new AddonMaster.MiragePrismExecute(atk).Cast();
}
