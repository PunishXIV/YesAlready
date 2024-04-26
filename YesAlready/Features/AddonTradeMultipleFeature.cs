using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Memory;
using ECommons;
using ECommons.Automation;
using FFXIVClientStructs.FFXIV.Component.GUI;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;
internal class AddonTradeMultipleFeature : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, "TradeMultiple", AddonUpdate);
    }

    public override void Disable()
    {
        base.Disable();
        AddonLifecycle.UnregisterListener(AddonUpdate);
    }

    protected static unsafe void AddonUpdate(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active || !P.Config.TradeMultiple)
            return;

        var addon = (AtkUnitBase*)addonInfo.Addon;
        if (MemoryHelper.ReadSeStringNullTerminated(new nint(addon->AtkValues[0].String)).ToString() != "5/5")
            return;

        if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("SelectYesno", out var _))
            return;

        Callback.Fire(addon, true, 0);
    }
}
