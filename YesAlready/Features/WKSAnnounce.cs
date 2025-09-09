using ECommons.GameHelpers;

namespace YesAlready.Features;

[AddonFeature(AddonEvent.PreUpdate)]
internal class WKSAnnounce : AddonFeature
{
    protected override bool IsEnabled() => C.WKSAnnounceHide;
    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk)
    {
        if (Player.Territory is not 1237) return; // I think second moon broke this and I don't have a sub to figure it out so only work on moon1
        atk->IsVisible = atk->AtkValues[1].UInt is not 0 and not 5 and not 6;
    }
}
