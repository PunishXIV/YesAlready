using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class AddonItemInspectionResultFeature : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "ItemInspectionResult", AddonSetup);
    }

    public override void Disable()
    {
        base.Disable();
        Svc.AddonLifecycle.UnregisterListener(AddonSetup);
    }

    private int itemInspectionCount = 0;

    protected unsafe void AddonSetup(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active || !P.Config.ItemInspectionResultEnabled) return;

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
            var rateLimiter = P.Config.ItemInspectionResultRateLimiter;
            if (rateLimiter != 0 && itemInspectionCount % rateLimiter == 0)
            {
                itemInspectionCount = 0;
                Utils.SEString.PrintPluginMessage("Rate limited, pausing item inspection loop.");
                return;
            }

            am.Next();
        }
    }
}
