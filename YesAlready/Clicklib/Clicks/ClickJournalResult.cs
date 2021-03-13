using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace Clicklib.Clicks
{
    internal sealed class ClickJournalResult : ClickBase
    {
        protected override string Name => "JournalResult";
        protected override string AddonName => "JournalResult";

        public unsafe ClickJournalResult(DalamudPluginInterface pluginInterface) : base(pluginInterface)
        {
            AvailableClicks["journal_result_complete"] = (addon) => SendClick(addon, EventType.CHANGE, 1, ((AddonJournalResult*)addon)->CompleteButton->AtkComponentBase.OwnerNode);
            AvailableClicks["journal_result_decline"] = (addon) => SendClick(addon, EventType.CHANGE, 2, ((AddonJournalResult*)addon)->DeclineButton->AtkComponentBase.OwnerNode);
        }
    }
}
