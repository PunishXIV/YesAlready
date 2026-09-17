namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
[Bother(nameof(Configuration.GuildLeveDifficultyConfirm), BotherCategory.Other, "Automatically confirms guild leves upon initiation at the highest difficulty.")]
internal class GuildLeveDifficulty : AddonFeature
{
    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo)
    {
        var addon = addonInfo.GetAddon<AddonGuildLeveDifficulty>();
        Callback.Fire(&addon->AtkUnitBase, true, 0, addon->DifficultySlider->MaxValue);
    }
}
