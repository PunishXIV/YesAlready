using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons.Automation;
using Lumina.Excel.GeneratedSheets;
using System.Linq;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class AddonPurifyResult : BaseFeature
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

    protected static unsafe void AddonUpdate(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active || !P.Config.AetherialReductionResults) return;

        var addon = addonInfo.Base();
        if (addon->UldManager.NodeList[17]->GetAsAtkTextNode()->NodeText.ToString() == Svc.Data.GetExcelSheet<Addon>().First(x => x.RowId == 2171).Text.RawString)
        {
            Svc.Log.Debug("Closing Purify Results menu");
            Callback.Fire(addon, true, -1);
        }
    }
}
