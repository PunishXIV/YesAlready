namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class ContentsFinderConfirm : AddonFeature
{
    protected override bool IsEnabled() => P.Config.ContentsFinderConfirmEnabled;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk)
    {
        new AddonMaster.ContentsFinderConfirm(atk).Commence();

        if (P.Config.ContentsFinderOneTimeConfirmEnabled)
        {
            P.Config.ContentsFinderConfirmEnabled = false;
            P.Config.ContentsFinderOneTimeConfirmEnabled = false;
            P.Config.Save();
        }
    }
}
