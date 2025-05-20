namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class ContentsFinderConfirm : AddonFeature
{
    protected override bool IsEnabled() => C.ContentsFinderConfirmEnabled;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk)
    {
        new AddonMaster.ContentsFinderConfirm(atk).Commence();

        if (C.ContentsFinderOneTimeConfirmEnabled)
        {
            C.ContentsFinderConfirmEnabled = false;
            C.ContentsFinderOneTimeConfirmEnabled = false;
            C.Save();
        }
    }
}
