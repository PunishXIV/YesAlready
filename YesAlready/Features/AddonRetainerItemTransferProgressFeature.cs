using Dalamud.Memory;
using Dalamud.Plugin.Services;
using ECommons.Automation;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;
using System.Linq;
using YesAlready.BaseFeatures;
using static ECommons.GenericHelpers;

namespace YesAlready.Features;

internal class AddonRetainerItemTransferProgressFeature : BaseFeature
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
        if (!P.Active || !P.Config.RetainerTransferProgressConfirm)
            return;

        if (TryGetAddonByName<AtkUnitBase>("RetainerItemTransferProgress", out var addon))
        {
            if (MemoryHelper.ReadSeStringNullTerminated(new nint(addon->AtkValues[0].String)).ToString() == Svc.Data.GetExcelSheet<Addon>().First(x => x.RowId == 13528).Text.RawString)
            {
                Svc.Log.Debug("Closing Entrust Duplicates menu");
                Callback.Fire(addon, true, -1);
            }
        }
    }
}
