using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons.Automation;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;
internal class AddonDKTFeature : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "LobbyDKTCheck", AddonSetup);
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "LobbyDKTCheckExec", AddonSetup);
    }

    public override void Disable()
    {
        base.Disable();
        Svc.AddonLifecycle.UnregisterListener(AddonSetup);
    }

    protected static unsafe void AddonSetup(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active || !P.Config.DataCentreTravelConfirmEnabled) return;
        Callback.Fire(addonInfo.Base(), true, 0);
    }
}
