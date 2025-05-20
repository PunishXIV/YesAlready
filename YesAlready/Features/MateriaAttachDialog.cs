using Dalamud.Game.ClientState.Conditions;

namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class MateriaAttachDialog : AddonFeature
{
    protected override bool IsEnabled() => C.MaterialAttachDialogEnabled;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk)
    {
        if (GenericHelpers.TryGetAddonMaster<AddonMaster.MateriaAttachDialog>(out var am))
        {
            if (C.OnlyMeldWhenGuaranteed && am.SuccessRateFloat < 100)
            {
                PluginLog.Debug($"Success rate {am.SuccessRateFloat} less than 100%, aborting meld.");
                return;
            }

            Service.TaskManager.Enqueue(() => Svc.Condition[ConditionFlag.MeldingMateria]);
            Service.TaskManager.Enqueue(am.Meld);
        }
    }
}
