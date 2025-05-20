namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostDraw)]
internal class RetainerItemTransferList : AddonFeature
{
    protected override bool IsEnabled() => C.RetainerTransferListConfirm;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk) => new AddonMaster.RetainerItemTransferList(atk).Confirm();
}
