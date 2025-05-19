namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class RaceChocoboResult : AddonFeature
{
    protected override bool IsEnabled() => P.Config.ChocoboRacingQuit;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk) => Callback.Fire(atk, true, 1);
}
