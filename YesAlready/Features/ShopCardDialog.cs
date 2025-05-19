namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class ShopCardDialog : AddonFeature
{
    protected override bool IsEnabled() => P.Config.ShopCardDialog;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk)
    {
        var addon = new AddonMaster.ShopCardDialog(atk);
        addon.Quantity = addon.MaxQuantity;
        addon.Sell();
    }
}
