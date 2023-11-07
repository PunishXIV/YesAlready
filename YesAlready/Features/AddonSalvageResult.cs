using Dalamud.Plugin.Services;
using ECommons.Automation;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Component.GUI;
using YesAlready.BaseFeatures;
using static ECommons.GenericHelpers;

namespace YesAlready.Features;

internal class AddonSalvageResult : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        Svc.Framework.Update += AddonListener;
    }

    public override void Disable()
    {
        base.Disable();
        Svc.Framework.Update -= AddonListener;
    }

    protected static unsafe void AddonListener(IFramework framework)
    {
        if (!P.Active || !P.Config.DesynthesisResults)
            return;

        if (TryGetAddonByName<AtkUnitBase>("SalvageResult", out var addon))
        {
            if (addon->AtkValues[17].Byte == 0)
            {
                Svc.Log.Debug("Closing Salvage Auto Results menu");
                Callback.Fire(addon, true, 1);
            }
        }
    }
}
