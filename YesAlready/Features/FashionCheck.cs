namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class FashionCheck : AddonFeature
{
    protected override unsafe bool IsEnabled() => P.Config.FashionCheckQuit && !GenericHelpers.TryGetAddonByName<AtkUnitBase>("ContentsInfo", out var _); // do not fire when the timers window is also open

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk) => Callback.Fire(atk, true, -1);
}
