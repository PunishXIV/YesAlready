using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.ClientState.Conditions;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class MateriaAttachDialog : BaseFeature
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
        if (GenericHelpers.TryGetAddonMaster<AddonMaster.MateriaAttachDialog>(out var am))
        {
            if (P.Config.OnlyMeldWhenGuaranteed && am.SuccessRateFloat < 100)
            {
                Svc.Log.Debug($"Success rate {am.SuccessRateFloat} less than 100%, aborting meld.");
                return;
            }

            P.TaskManager.Enqueue(() => Svc.Condition[ConditionFlag.MeldingMateria]);
            P.TaskManager.Enqueue(am.Meld);
        }
    }
}
