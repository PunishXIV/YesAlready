using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class SalvageDialog : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "SalvageDialog", AddonSetup);
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PreSetup, "SalvageDialog", AddonSetup);
    }

    public override void Disable()
    {
        base.Disable();
        Svc.AddonLifecycle.UnregisterListener(AddonSetup);
    }

    protected unsafe void AddonSetup(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active || !GenericHelpers.IsAddonReady(addonInfo.Base())) return;

        if (GenericHelpers.TryGetAddonMaster<AddonMaster.SalvageDialog>(out var am))
        {
            switch (eventType)
            {
                case AddonEvent.PreSetup:
                    if (P.Config.DesynthBulkDialogEnabled)
                        am.Addon->AtkValues[20].SetBool(true);
                    break;
                case AddonEvent.PostSetup:
                    if (P.Config.DesynthDialogEnabled)
                    {
                        am.Checkbox();
                        am.Desynthesize();
                    }
                    break;
            }
        }
    }
}
