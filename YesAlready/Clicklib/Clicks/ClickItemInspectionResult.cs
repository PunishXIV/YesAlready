using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace Clicklib.Clicks
{
    internal sealed class ClickItemInspectionResult : ClickBase
    {
        protected override string Name => "ItemInspectionResult";
        protected override string AddonName => "ItemInspectionResult";

        public unsafe ClickItemInspectionResult(DalamudPluginInterface pluginInterface) : base(pluginInterface)
        {
            AvailableClicks["item_inspection_result_next"] = (addon) => SendClick(addon, EventType.CHANGE, 0, ((AtkComponentButton*)((AddonItemInspectionResult*)addon)->AtkUnitBase.ULDData.NodeList[2])->AtkComponentBase.OwnerNode);
            AvailableClicks["item_inspection_result_close"] = (addon) => SendClick(addon, EventType.CHANGE, 0xFFFFFFFF, ((AtkComponentButton*)((AddonItemInspectionResult*)addon)->AtkUnitBase.ULDData.NodeList[3])->AtkComponentBase.OwnerNode);
        }
    }
}
