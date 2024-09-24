using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;
public class MiragePrismRemove : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "MiragePrismRemove", AddonSetup);
    }

    public override void Disable()
    {
        base.Disable();
        Svc.AddonLifecycle.UnregisterListener(AddonSetup);
    }

    protected static unsafe void AddonSetup(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active || !P.Config.MiragePrismRemoveDispel) return;
        new AddonMaster.MiragePrismRemove(addonInfo.Base()).Dispel();
    }
}
