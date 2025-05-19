namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class FGSEnterDialog : AddonFeature
{
    protected override bool IsEnabled() => P.Config.FallGuysRegisterConfirm;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk) => Callback.Fire(atk, true, 0);
}
