namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class MateriaRetrieveDialog : AddonFeature
{
    protected override bool IsEnabled() => C.MateriaRetrieveDialogEnabled;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk) => new AddonMaster.MateriaRetrieveDialog(atk).Begin();
}
