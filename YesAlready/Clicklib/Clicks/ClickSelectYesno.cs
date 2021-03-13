using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace Clicklib.Clicks
{
    internal sealed class ClickSelectYesNo : ClickBase
    {
        protected override string Name => "SelectYesno";
        protected override string AddonName => "SelectYesno";

        public unsafe ClickSelectYesNo(DalamudPluginInterface pluginInterface) : base(pluginInterface)
        {
            AvailableClicks["select_yes"] = (addon) => SendClick(addon, EventType.CHANGE, 0, ((AddonSelectYesno*)addon)->YesButton->AtkComponentBase.OwnerNode);
            AvailableClicks["select_no"] = (addon) => SendClick(addon, EventType.CHANGE, 1, ((AddonSelectYesno*)addon)->NoButton->AtkComponentBase.OwnerNode);
        }
    }
}
