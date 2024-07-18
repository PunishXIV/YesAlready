using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons;
using ECommons.UIHelpers.AddonMasterImplementations;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class AddonSalvageDialogFeature : BaseFeature
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

        var addon = new AddonMaster.SalvageDialog(addonInfo.Base());
        switch (eventType)
        {
            case AddonEvent.PreSetup:
                if (P.Config.DesynthBulkDialogEnabled)
                    addon.Addon->AtkValues[20].SetBool(true);
                break;
            case AddonEvent.PostSetup:
                if (P.Config.DesynthDialogEnabled)
                {
                    addon.Checkbox();
                    addon.Desynthesize();
                }
                break;
        }
    }
}
