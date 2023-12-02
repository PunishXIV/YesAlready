using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ClickLib.Clicks;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class AddonRetainerItemTransferListFeature : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        AddonLifecycle.RegisterListener(AddonEvent.PostDraw, "RetainerItemTransferList", AddonUpdate);
    }

    public override void Disable()
    {
        base.Disable();
        AddonLifecycle.UnregisterListener(AddonUpdate);
    }

    protected static unsafe void AddonUpdate(AddonEvent eventType, AddonArgs addonInfo)
    {
        var addon = (AtkUnitBase*)addonInfo.Addon;

        if (!P.Active || !P.Config.RetainerTransferListConfirm)
            return;
        

        ClickRetainerItemTransferList.Using((nint)addon).Confirm();
    }
}
