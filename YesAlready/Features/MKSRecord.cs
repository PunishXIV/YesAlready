namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class MKSRecord : AddonFeature
{
    protected override bool IsEnabled() => C.MKSRecordQuit;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk) => Callback.Fire(atk, true, -1);
}
