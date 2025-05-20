namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class FGSExitDialog : AddonFeature
{
    protected override bool IsEnabled() => C.FallGuysExitConfirm;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk) => Callback.Fire(atk, true, 0);
}
