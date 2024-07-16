using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.ClientState.Conditions;
using ECommons.UIHelpers.AddonMasterImplementations;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class AddonMateriaAttachDialogFeature : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "MateriaAttachDialog", AddonSetup);
    }

    public override void Disable()
    {
        base.Disable();
        Svc.AddonLifecycle.UnregisterListener(AddonSetup);
    }

    protected static unsafe void AddonSetup(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active || !P.Config.MaterialAttachDialogEnabled) return;
        var addon = new AddonMaster.MateriaAttachDialog(addonInfo.Base());

        if (P.Config.OnlyMeldWhenGuaranteed && addon.SuccessRateFloat < 100)
        {
            Svc.Log.Debug($"Success rate {addon.SuccessRateFloat} less than 100%, aborting meld.");
            return;
        }

        P.TaskManager.Enqueue(() => Svc.Condition[ConditionFlag.MeldingMateria]);
        P.TaskManager.Enqueue(addon.Meld);
    }
}
