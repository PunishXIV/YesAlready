namespace YesAlready.Features;

[AddonFeature(AddonEvent.PreUpdate)]
internal class WKSAnnounce : AddonFeature
{
    protected override bool IsEnabled() => C.WKSAnnounceHide;
    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk) => atk->IsVisible = atk->AtkValues[1].UInt is not 0 and not 5 and not 6;
}
