using System.Linq;

namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
[AddonFeature(AddonEvent.PostUpdate)]
internal class HWDLottery : AddonFeature
{
    protected override bool IsEnabled() => P.Config.KupoOfFortune;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk)
    {
        switch (eventType)
        {
            case AddonEvent.PostSetup:
                Callback.Fire(atk, true, 0, 1);
                break;
            case AddonEvent.PostUpdate:
                var closeButton = atk->UldManager.NodeList[7]->GetAsAtkComponentButton();
                if (Enumerable.Range(32, 5).Select(i => atk->AtkValues[i].UInt).ToList().All(x => x != 0) && closeButton != null && closeButton->IsEnabled)
                {
                    var eventData = new AtkEvent();
                    var inputData = stackalloc int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                    atk->ReceiveEvent(AtkEventType.ButtonClick, 0, &eventData);
                }
                break;
        }
    }
}
