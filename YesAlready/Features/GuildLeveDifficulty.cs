namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class GuildLeveDifficulty : AddonFeature
{
    protected override bool IsEnabled() => C.GuildLeveDifficultyConfirm;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk) => Callback.Fire(atk, true, 0, atk->AtkValues[1].Int);
}
