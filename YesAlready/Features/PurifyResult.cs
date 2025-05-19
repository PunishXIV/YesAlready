using Dalamud.Memory;
using Lumina.Excel.Sheets;
using System.Linq;

namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostUpdate)]
internal class PurifyResult : AddonFeature
{
    protected override bool IsEnabled() => P.Config.AetherialReductionResults;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk)
    {
        if (!GenericHelpers.IsAddonReady(atk)) return;

        if (MemoryHelper.ReadSeString(&atk->GetTextNodeById(2)->NodeText).GetText() == Svc.Data.GetExcelSheet<Addon>().First(x => x.RowId == 2171).Text)
        {
            PluginLog.Debug("Closing Purify Results menu");
            Callback.Fire(atk, true, -1);
        }
    }
}
