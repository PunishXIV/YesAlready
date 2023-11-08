using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Addon.Lifecycle;
using ECommons.Automation;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;
using System.Linq;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class AddonPurifyResult : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, "PurifyResult", AddonUpdate);
    }

    public override void Disable()
    {
        base.Disable();
        AddonLifecycle.UnregisterListener(AddonUpdate);
    }

    protected static unsafe void AddonUpdate(AddonEvent eventType, AddonArgs addonInfo)
    {
        var addon = (AtkUnitBase*)addonInfo.Addon;

        if (!P.Active || !P.Config.AetherialReductionResults)
            return;

        if (addon->UldManager.NodeList[17]->GetAsAtkTextNode()->NodeText.ToString() == Svc.Data.GetExcelSheet<Addon>().First(x => x.RowId == 2171).Text.RawString)
        {
            Svc.Log.Debug("Closing Purify Results menu");
            Callback.Fire(addon, true, -1);
        }
    }
}
