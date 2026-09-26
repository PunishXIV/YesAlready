using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;
using System.Linq;

namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
[AddonFeature(AddonEvent.PreFinalize)]
internal class SelectIconString : TextMatchingFeature
{
    protected override unsafe string GetSetLastSeenText(AtkUnitBase* atk)
    {
        var entries = PopupMenuEntries.GetIndexed((PopupMenu*)&((AddonSelectIconString*)atk)->PopupMenu);
        Service.Watcher.LastSeenListEntries = [.. entries];
        return string.Join(", ", entries.Select(x => x.Text));
    }

    protected override unsafe object? ShouldProceed(string text, AtkUnitBase* atk)
    {
        string[] entries = PopupMenuEntries.GetTexts((PopupMenu*)&((AddonSelectIconString*)atk)->PopupMenu);

        if (C.SelectStringAutoAcceptQuests)
            foreach (var e in entries)
                if (Service.Quests.Any(q => q.Value.Equals(e) && Quest.GetRow(q.Key).PreviousQuest.All(q => q.RowId is not 0 && QuestManager.IsQuestComplete(q.RowId))))
                    return GetMatchingIndex(entries, e, false);

        var nodes = C.GetAllNodes().OfType<ListEntryNode>();
        foreach (var node in nodes)
        {
            if (!node.Enabled || string.IsNullOrEmpty(node.Text))
                continue;

            if (!CheckRestrictions(node))
                continue;

            if (Service.Watcher.LastSelectedListEntry is { } last && last.TargetDataId == Svc.Targets.Target?.BaseId && last.Node == node)
                continue;

            var index = GetMatchingIndex(entries, node.Text, node.IsTextRegex);
            if (index.HasValue)
            {
                Service.Watcher.LastSelectedListEntry = new() { TargetDataId = Svc.Targets.Target?.BaseId ?? 0, Node = node };
                return index.Value;
            }
        }

        return null;
    }

    protected override unsafe void Proceed(AtkUnitBase* atk, object? matchingNode)
    {
        if (matchingNode is not int index) return;
        Callback.Fire(atk, true, index);
    }
}
