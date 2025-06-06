namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class FrontlineRecord : AddonFeature
{
    protected override bool IsEnabled() => C.FrontlineRecordQuit;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk) => Callback.Fire(atk, true, -1);
}
