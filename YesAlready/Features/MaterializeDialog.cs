namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class MaterializeDialog : AddonFeature
{
    protected override bool IsEnabled() => P.Config.MaterializeDialogEnabled;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk) => new AddonMaster.MaterializeDialog(atk).Materialize();
}
