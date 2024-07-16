using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons.UIHelpers.AddonMasterImplementations;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class AddonRetainerTaskAskFeature : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "RetainerTaskAsk", AddonSetup);
    }

    public override void Disable()
    {
        base.Disable();
        Svc.AddonLifecycle.UnregisterListener(AddonSetup);
    }

    protected static unsafe void AddonSetup(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active || !P.Config.RetainerTaskAskEnabled) return;
        var addon = new AddonMaster.RetainerTaskAsk(addonInfo.Base());
        P.TaskManager.Enqueue(() => addon.AssignButton->IsEnabled); // must be throttled, there's a little delay after setup before this is enabled
        P.TaskManager.Enqueue(addon.Assign);
    }
}
