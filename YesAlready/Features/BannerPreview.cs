using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons.Automation;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;
internal class BannerPreview : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "BannerPreview", AddonSetup);
    }

    public override void Disable()
    {
        base.Disable();
        Svc.AddonLifecycle.UnregisterListener(AddonSetup);
    }

    protected static unsafe void AddonSetup(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active || !P.Config.BannerPreviewUpdate) return;
        Callback.Fire(addonInfo.Base(), true, 0);
        // FIX: this causes a "Character not in frame error" when done PostSetup ... somehow
        //new AddonMaster.BannerPreview(addonInfo.Base()).Update();
    }
}
