using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons.Automation;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class AddonFrontlineRecord : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "FrontlineRecord", AddonSetup);
    }

    public override void Disable()
    {
        base.Disable();
        Svc.AddonLifecycle.UnregisterListener(AddonSetup);
    }

    protected unsafe void AddonSetup(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active || !P.Config.FrontlineRecordQuit) return;
        Callback.Fire(addonInfo.Base(), true, -1);
    }
}
