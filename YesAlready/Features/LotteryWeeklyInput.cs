using System;

namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class LotteryWeeklyInput : AddonFeature
{
    protected override bool IsEnabled() => C.LotteryWeeklyInput;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk) => Callback.Fire(atk, true, new Random().Next(0, 10000));
}
