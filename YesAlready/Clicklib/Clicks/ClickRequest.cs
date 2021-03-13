using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace Clicklib.Clicks
{
    internal sealed class ClickRequest : ClickBase
    {
        protected override string Name => "Request";
        protected override string AddonName => "Request";

        public unsafe ClickRequest(DalamudPluginInterface pluginInterface) : base(pluginInterface)
        {
            AvailableClicks["request_hand_over"] = (addon) => SendClick(addon, EventType.CHANGE, 0, ((AddonRequest*)addon)->HandOverButton);
            AvailableClicks["request_cancel"] = (addon) => SendClick(addon, EventType.CHANGE, 1, ((AddonRequest*)addon)->CancelButton);
        }
    }
}
