using Lumina.Excel.Sheets;

namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
[Bother(nameof(Configuration.RetainerTaskResultEnabled), BotherCategory.Retainers, "Automatically send a retainer on the same venture as before when receiving an item.")]
internal class RetainerTaskResult : AddonFeature
{
    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo)
    {
        var addon = addonInfo.GetAddon<AddonRetainerTaskResult>();
        if (addon->ReassignButton->ButtonTextNode->NodeText.GetText() == Addon.GetRow(2365).Text.ToString()) // I was using addon->ResultMode but I think that's not ready at post setup. cba checking
        {
            Log($"Reassign not available. Skipping.");
            return;
        }

        Service.TaskManager.Enqueue(() => addon->ReassignButton->IsEnabled);
        Service.TaskManager.Enqueue(() => addon->ReassignButton->Click());
    }
}
