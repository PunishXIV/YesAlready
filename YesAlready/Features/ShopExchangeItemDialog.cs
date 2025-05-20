namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class ShopExchangeItemDialog : AddonFeature
{
    protected override bool IsEnabled() => C.ShopExchangeItemDialogEnabled;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk) => Callback.Fire(atk, true, 0);
}
