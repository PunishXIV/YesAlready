namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class RetainerTaskAsk : AddonFeature
{
    protected override bool IsEnabled() => C.RetainerTaskAskEnabled;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk)
    {
        if (GenericHelpers.TryGetAddonMaster<AddonMaster.RetainerTaskAsk>(out var am))
        {
            Service.TaskManager.Enqueue(() => am.AssignButton->IsEnabled); // must be throttled, there's a little delay after setup before this is enabled
            Service.TaskManager.Enqueue(am.Assign);
        }
    }
}
