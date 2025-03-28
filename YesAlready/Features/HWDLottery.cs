using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons.Automation;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Linq;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;
internal class HWDLottery : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "HWDLottery", AddonSetup);
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, "HWDLottery", AddonUpdate);
    }

    public override void Disable()
    {
        base.Disable();
        Svc.AddonLifecycle.UnregisterListener(AddonSetup);
        Svc.AddonLifecycle.UnregisterListener(AddonUpdate);
    }

    protected static unsafe void AddonSetup(AddonEvent eventType, AddonArgs args)
    {
        if (!P.Active || !P.Config.KupoOfFortune) return;
        Callback.Fire(args.Base(), true, 0, 1);
    }

    private unsafe void AddonUpdate(AddonEvent type, AddonArgs args)
    {
        if (!P.Active || !P.Config.KupoOfFortune) return;

        var addon = args.Base();
        var closeButton = addon->UldManager.NodeList[7]->GetAsAtkComponentButton();
        if (Enumerable.Range(32, 5).Select(i => addon->AtkValues[i].UInt).ToList().All(x => x != 0) && closeButton != null && closeButton->IsEnabled)
        {
            var eventData = new AtkEvent();
            var inputData = stackalloc int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            addon->ReceiveEvent(AtkEventType.ButtonClick, 0, &eventData);
        }
    }
}
