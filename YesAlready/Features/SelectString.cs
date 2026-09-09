using System.Linq;

namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
[AddonFeature(AddonEvent.PreFinalize)]
internal class SelectString : TextMatchingFeature
{
    protected override unsafe string GetSetLastSeenText(AtkUnitBase* atk)
    {
        var entries = PopupMenuEntries.GetIndexed((PopupMenu*)&((AddonSelectString*)atk)->PopupMenu);
        Service.Watcher.LastSeenListEntries = [.. entries];
        return string.Join(", ", entries.Select(x => x.Text));
    }

    protected override unsafe object? ShouldProceed(string text, AtkUnitBase* atk)
    {
        string[] entries = PopupMenuEntries.GetTexts((PopupMenu*)&((AddonSelectString*)atk)->PopupMenu);

        var nodes = C.GetAllNodes().OfType<ListEntryNode>();
        foreach (var node in nodes)
        {
            if (!node.Enabled || string.IsNullOrEmpty(node.Text))
                continue;

            if (!CheckRestrictions(node))
                continue;

            if (Service.Watcher.LastSelectedListEntry is { } last && last.TargetDataId == Svc.Targets.Target?.BaseId && last.Node == node)
            {
                Log($"Skipping match because it was the same as last");
                continue;
            }

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
