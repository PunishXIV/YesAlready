//using Dalamud.Memory;

//namespace YesAlready.Features;

//[AddonFeature(AddonEvent.PostUpdate)]
//internal class TradeMultiple : AddonFeature
//{
//    protected override bool IsEnabled() => C.TradeMultiple;

//    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk)
//    {
//        if (MemoryHelper.ReadSeStringNullTerminated(new nint(atk->AtkValues[0].String)).ToString() != "5/5")
//            return;

//        if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("SelectYesno", out var _))
//            return;

//        Callback.Fire(atk, true, 0);
//    }
//}
