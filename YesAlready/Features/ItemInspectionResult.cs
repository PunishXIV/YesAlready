using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Memory;

namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
[Bother(nameof(Configuration.ItemInspectionResultEnabled), BotherCategory.Forays,
    "Eureka/Bozja lockboxes, forgotten fragments, and more. Warning: this does not check if you are maxed on items. Rate limiter (pause after N items).")]
internal class ItemInspectionResult : AddonFeature
{
    private int itemInspectionCount = 0;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo)
    {
        var addon = addonInfo.GetAddon<AddonItemInspectionResult>();
        if (addon->UldManager.NodeListCount < 64) return;

        var nameNode = addon->GetTextNodeById(26);
        var descNode = addon->GetTextNodeById(35);
        if (nameNode == null || descNode == null || !nameNode->AtkResNode.IsVisible() || !descNode->AtkResNode.IsVisible())
            return;

        var values = addon->TypedAtkValues;
        var description = values->Description.Type == AtkValueType.String ? MemoryHelper.ReadSeStringNullTerminated((nint)values->Description.String.Value).GetText() : descNode->NodeText.GetText();
        if (description.Contains('※') || description.Contains("liées à Garde-la-Reine"))
        {
            Svc.Chat.PrintPluginMessage(new SeString(new TextPayload("Received: "), new TextPayload(nameNode->NodeText.GetText())));
        }

        itemInspectionCount++;
        var rateLimiter = C.ItemInspectionResultRateLimiter;
        if (rateLimiter != 0 && itemInspectionCount % rateLimiter == 0)
        {
            itemInspectionCount = 0;
            Svc.Chat.PrintPluginMessage("Rate limited, pausing item inspection loop.");
            return;
        }

        var nextButton = addon->GetComponentButtonById(74);
        var closeButton = addon->GetComponentButtonById(73);
        if (values->HasNext.Int != 0 && nextButton->IsEnabled)
            nextButton->Click();
        else
            closeButton->Click();
    }
}
