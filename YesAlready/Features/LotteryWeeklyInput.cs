using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons.Automation;
using System;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;
internal class LotteryWeeklyInput : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "LotteryWeeklyInput", AddonSetup);
    }

    public override void Disable()
    {
        base.Disable();
        Svc.AddonLifecycle.UnregisterListener(AddonSetup);
    }

    protected static unsafe void AddonSetup(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active || !P.Config.LotteryWeeklyInput) return;
        Callback.Fire(addonInfo.Base(), true, new Random().Next(0, 10000));
    }
}
