using ClickLib.Clicks;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class AddonRetainerTaskResultFeature : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "RetainerTaskResult", AddonSetup);
    }

    public override void Disable()
    {
        base.Disable();
        AddonLifecycle.UnregisterListener(AddonSetup);
    }

    protected unsafe void AddonSetup(AddonEvent eventType, AddonArgs addonInfo)
    {
        var addon = (AtkUnitBase*)addonInfo.Addon;

        if (!P.Config.Enabled || !P.Config.RetainerTaskResultEnabled)
            return;

        var addonPtr = (AddonRetainerTaskResult*)addon;
        var buttonText = addonPtr->ReassignButton->ButtonTextNode->NodeText.ToString();
        if (buttonText == Svc.Data.GetExcelSheet<Addon>(Svc.ClientState.ClientLanguage).GetRow(2365).Text)
            return;

        ClickRetainerTaskResult.Using((nint)addon).Reassign();
    }
}
