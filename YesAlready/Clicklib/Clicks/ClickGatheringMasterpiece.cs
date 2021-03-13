using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace Clicklib.Clicks
{
    internal class ClickGatheringMasterpiece : ClickBase
    {
        protected override string Name => "Collectables";
        protected override string AddonName => "GatheringMasterpiece";

        public unsafe ClickGatheringMasterpiece(DalamudPluginInterface pluginInterface) : base(pluginInterface)
        {
            AvailableClicks["collect"] = (addon) => SendClick(addon, EventType.ICON_TEXT_ROLL_OUT, 112, ((AddonGatheringMasterpiece*)addon)->CollectDragDrop->AtkComponentBase.OwnerNode);
        }
    }
}
