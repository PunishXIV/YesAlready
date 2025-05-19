namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
[AddonFeature(AddonEvent.PostUpdate, "SalvageAutoDialog")]
internal class SalvageResult : AddonFeature
{
    protected override bool IsEnabled() => P.Config.DesynthesisResults;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk)
    {
        if (!GenericHelpers.IsAddonReady(atk)) return;

        switch (addonInfo.AddonName)
        {
            case "SalvageResult":
                new AddonMaster.SalvageResult(atk).Close();
                break;

            case "SalvageAutoDialog":
                if (GenericHelpers.TryGetAddonMaster<AddonMaster.SalvageAutoDialog>(out var am) && am.DesynthesisInactive)
                {
                    am.EndDesynthesis();
                }
                break;
        }
    }
}
