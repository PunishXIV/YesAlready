using Lumina.Excel.Sheets;

namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class RetainerTaskResult : AddonFeature
{
    protected override bool IsEnabled() => C.RetainerTaskResultEnabled;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk)
    {
        if (GenericHelpers.TryGetAddonMaster<AddonMaster.RetainerTaskResult>(out var am))
        {
            var buttonText = am.ReassignButton->ButtonTextNode->NodeText.GetText();
            if (buttonText == Svc.Data.GetExcelSheet<Addon>(Svc.ClientState.ClientLanguage).GetRow(2365).Text) // Recall
                return;

            Service.TaskManager.Enqueue(() => am.ReassignButton->IsEnabled); // must be throttled, there's a little delay after setup before this is enabled
            Service.TaskManager.Enqueue(am.Reassign);
        }
    }
}
