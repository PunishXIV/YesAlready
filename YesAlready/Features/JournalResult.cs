namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class JournalResult : AddonFeature
{
    protected override bool IsEnabled() => C.JournalResultCompleteEnabled;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk) => new AddonMaster.JournalResult(atk).Complete();
}
