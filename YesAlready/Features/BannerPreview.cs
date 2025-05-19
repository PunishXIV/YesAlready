namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class BannerPreview : AddonFeature
{
    protected override bool IsEnabled() => P.Config.BannerPreviewUpdate;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk) => Callback.Fire(atk, true, 0);// FIX: this causes a "Character not in frame error" when done PostSetup ... somehow//new AddonMaster.BannerPreview(atk).Update();
}
