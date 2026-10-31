namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
[AddonFeature(AddonEvent.PostUpdate)]
[Bother(nameof(Configuration.KupoOfFortune), BotherCategory.Minigames, "Automatically select a kupo of fortune reward. This will instantly complete a single kupo ticket but is unable to continue to the next automatically.")]
public class HWDLottery : AddonFeature
{
    public enum Side
    {
        Left,
        Right,
    }

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo)
    {
        switch (eventType)
        {
            case AddonEvent.PostSetup:
                Callback.Fire(addonInfo.GetAddon<AtkUnitBase>(), true, 0, 1);
                break;
            case AddonEvent.PostUpdate:
                var addon = addonInfo.GetAddon<AddonHWDLottery>();
                if (addon->Stage == 3 && addon->CloseButton != null && addon->CloseButton->IsEnabled)
                    addon->CloseButton->Click();
                break;
        }
    }
}
