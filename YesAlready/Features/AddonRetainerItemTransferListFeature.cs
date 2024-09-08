using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class AddonRetainerItemTransferListFeature : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostDraw, "RetainerItemTransferList", AddonUpdate);
    }

    public override void Disable()
    {
        base.Disable();
        Svc.AddonLifecycle.UnregisterListener(AddonUpdate);
    }

    protected static unsafe void AddonUpdate(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active || !P.Config.RetainerTransferListConfirm) return;
        new AddonMaster.RetainerItemTransferList(addonInfo.Base()).Confirm();
    }
}
