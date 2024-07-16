using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Memory;
using ECommons.UIHelpers.AddonMasterImplementations;
using Lumina.Excel.GeneratedSheets;
using System.Linq;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class AddonRetainerItemTransferProgressFeature : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, "RetainerItemTransferProgress", AddonUpdate);
    }

    public override void Disable()
    {
        base.Disable();
        Svc.AddonLifecycle.UnregisterListener(AddonUpdate);
    }

    protected static unsafe void AddonUpdate(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active || !P.Config.RetainerTransferProgressConfirm) return;

        var addon = new AddonMaster.RetainerItemTransferProgress(addonInfo.Base());
        if (MemoryHelper.ReadSeStringNullTerminated(new nint(addon.Base->AtkValues[0].String)).ToString() == Svc.Data.GetExcelSheet<Addon>().First(x => x.RowId == 13528).Text.RawString)
        {
            Svc.Log.Debug("Closing Entrust Duplicates menu");
            addon.Close();
        }
    }
}
