namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class LovmResult : AddonFeature
{
    protected override bool IsEnabled() => C.LordOfVerminionQuit;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk) => Callback.Fire(atk, true, -1);
}
