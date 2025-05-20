namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class WKSReward : AddonFeature
{
    protected override bool IsEnabled() => C.WKSRewardClose;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk) => Callback.Fire(atk, true, -1);
}
