using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace Clicklib.Clicks
{
    internal sealed class ClickTalk : ClickBase
    {
        protected override string Name => "Talk";
        protected override string AddonName => "Talk";

        public unsafe ClickTalk(DalamudPluginInterface pluginInterface) : base(pluginInterface)
        {
            AvailableClicks["talk"] = (addon) => SendClick(addon, EventType.INPUT, 0, ((AddonTalk*)addon)->AtkStage);
        }
    }
}
