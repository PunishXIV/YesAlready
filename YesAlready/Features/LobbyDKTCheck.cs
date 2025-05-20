namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup, "LobbyDKTCheck")]
[AddonFeature(AddonEvent.PostSetup, "LobbyDKTCheckExec")]
internal class LobbyDKTCheck : AddonFeature
{
    protected override bool IsEnabled() => C.DataCentreTravelConfirmEnabled;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk) => Callback.Fire(atk, true, 0);
}
