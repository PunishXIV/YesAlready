namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class SalvageDialog : AddonFeature
{
    protected override bool IsEnabled() => C.DesynthDialogEnabled;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk)
    {
        if (!GenericHelpers.IsAddonReady(atk)) return;

        if (GenericHelpers.TryGetAddonMaster<AddonMaster.SalvageDialog>(out var am))
        {
            switch (eventType)
            {
                //case AddonEvent.PreSetup:
                //    if (C.DesynthBulkDialogEnabled && addonInfo is AddonSetupArgs { AtkValueCount: > 20 } args)
                //        args.AtkValueSpan.GetPointer(20)->SetBool(true);
                //    break;
                case AddonEvent.PostSetup:
                    if (C.DesynthDialogEnabled)
                    {
                        am.Checkbox();
                        am.Desynthesize();
                    }
                    break;
            }
        }
    }
}
