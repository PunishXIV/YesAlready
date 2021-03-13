using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace Clicklib.Clicks
{
    internal sealed class ClickSalvageDialog : ClickBase
    {
        protected override string Name => "SalvageDialog";
        protected override string AddonName => "SalvageDialog";

        public unsafe ClickSalvageDialog(DalamudPluginInterface pluginInterface) : base(pluginInterface)
        {
            AvailableClicks["desynthesize_checkbox"] = (addon) => SendClick(addon, EventType.CHANGE, 3, ((AddonSalvageDialog*)addon)->CheckBox->AtkComponentButton.AtkComponentBase.OwnerNode);
            AvailableClicks["desynthesize"] = (addon) => SendClick(addon, EventType.CHANGE, 1, ((AddonSalvageDialog*)addon)->DesynthesizeButton->AtkComponentBase.OwnerNode);
        }
    }
}
