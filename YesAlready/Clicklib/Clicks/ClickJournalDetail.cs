using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace Clicklib.Clicks
{
    internal sealed class ClickJournalDetail : ClickBase
    {
        protected override string Name => "JournalDetail";
        protected override string AddonName => "JournalDetail";

        public unsafe ClickJournalDetail(DalamudPluginInterface pluginInterface) : base(pluginInterface)
        {
            AvailableClicks["journal_detail_accept"] = (addon) => SendClick(addon, EventType.CHANGE, 1, ((AddonJournalDetail*)addon)->AcceptButton);
            AvailableClicks["journal_detail_decline"] = (addon) => SendClick(addon, EventType.CHANGE, 2, ((AddonJournalDetail*)addon)->AcceptButton);
        }
    }
}
