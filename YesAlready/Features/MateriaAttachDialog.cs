using Dalamud.Game.ClientState.Conditions;

namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class MateriaAttachDialog : AddonFeature
{
    protected override bool IsEnabled() => P.Config.MaterialAttachDialogEnabled;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk)
    {
        if (GenericHelpers.TryGetAddonMaster<AddonMaster.MateriaAttachDialog>(out var am))
        {
            if (P.Config.OnlyMeldWhenGuaranteed && am.SuccessRateFloat < 100)
            {
                PluginLog.Debug($"Success rate {am.SuccessRateFloat} less than 100%, aborting meld.");
                return;
            }

            P.TaskManager.Enqueue(() => Svc.Condition[ConditionFlag.MeldingMateria]);
            P.TaskManager.Enqueue(am.Meld);
        }
    }
}
