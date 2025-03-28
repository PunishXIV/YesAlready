using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Memory;
using ECommons.Automation;
using Lumina.Excel.Sheets;
using System.Linq;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class PurifyResult : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, "PurifyResult", AddonUpdate);
    }

    public override void Disable()
    {
        base.Disable();
        Svc.AddonLifecycle.UnregisterListener(AddonUpdate);
    }

    private static unsafe void AddonUpdate(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active || !P.Config.AetherialReductionResults || !GenericHelpers.IsAddonReady(addonInfo.Base())) return;

        var addon = addonInfo.Base();
        if (MemoryHelper.ReadSeString(&addon->GetTextNodeById(2)->NodeText).GetText() == Svc.Data.GetExcelSheet<Addon>().First(x => x.RowId == 2171).Text)
        {
            PluginLog.Debug("Closing Purify Results menu");
            Callback.Fire(addon, true, -1);
        }
    }
}
