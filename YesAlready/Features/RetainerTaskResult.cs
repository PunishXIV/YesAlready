using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Lumina.Excel.GeneratedSheets;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class RetainerTaskResult : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "RetainerTaskResult", AddonSetup);
    }

    public override void Disable()
    {
        base.Disable();
        Svc.AddonLifecycle.UnregisterListener(AddonSetup);
    }

    protected unsafe void AddonSetup(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active || !P.Config.RetainerTaskResultEnabled) return;

        if (GenericHelpers.TryGetAddonMaster<AddonMaster.RetainerTaskResult>(out var am))
        {
            var buttonText = am.ReassignButton->ButtonTextNode->NodeText.ToString();
            if (buttonText == Svc.Data.GetExcelSheet<Addon>(Svc.ClientState.ClientLanguage).GetRow(2365).Text)
                return;

            P.TaskManager.Enqueue(() => am.ReassignButton->IsEnabled); // must be throttled, there's a little delay after setup before this is enabled
            P.TaskManager.Enqueue(am.Reassign);
        }
    }
}
