using Dalamud.Plugin.Services;
using ECommons.Automation;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;
using System.Linq;
using YesAlready.BaseFeatures;
using static ECommons.GenericHelpers;

namespace YesAlready.Features;

internal class AddonPurifyResult : BaseFeature
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
        if (!P.Active || !P.Config.AetherialReductionResults)
            return;

        if (TryGetAddonByName<AtkUnitBase>("PurifyResult", out var addon))
        {
            if (addon->UldManager.NodeList[17]->GetAsAtkTextNode()->NodeText.ToString() == Svc.Data.GetExcelSheet<Addon>().First(x => x.RowId == 2171).Text.RawString)
            {
                Svc.Log.Debug("Closing Purify Results menu");
                Callback.Fire(addon, true, -1);
            }
        }
    }
}
