using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class ItemInspectionResut : AddonFeature
{
    private int itemInspectionCount = 0;

    protected override bool IsEnabled() => C.ItemInspectionResultEnabled;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk)
    {
        if (GenericHelpers.TryGetAddonMaster<AddonMaster.ItemInspectionResult>(out var am))
        {
            if (am.Base->UldManager.NodeListCount < 64) return;
            if (!am.NameNode->IsVisible() || !am.DescNode->IsVisible()) return;

            // This is hackish, but works well enough (for now).
            // Languages that dont contain the magic character will need special handling.
            if (am.Description.TextValue.Contains('※') || am.Description.TextValue.Contains("liées à Garde-la-Reine"))
            {
                am.ItemName.Payloads.Insert(0, new TextPayload("Received: "));
                Utils.SEString.PrintPluginMessage(am.ItemName);
            }

            itemInspectionCount++;
            var rateLimiter = C.ItemInspectionResultRateLimiter;
            if (rateLimiter != 0 && itemInspectionCount % rateLimiter == 0)
            {
                itemInspectionCount = 0;
                Utils.SEString.PrintPluginMessage("Rate limited, pausing item inspection loop.");
                return;
            }

            if (am.NextButton->IsEnabled)
                am.Next();
            else
                am.Close();
        }
    }
}
