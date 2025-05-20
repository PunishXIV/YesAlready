namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class FGSEnterDialog : AddonFeature
{
    protected override bool IsEnabled() => C.FallGuysRegisterConfirm;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk) => Callback.Fire(atk, true, 0);
}
