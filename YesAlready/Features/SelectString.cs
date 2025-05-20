using System.Linq;

namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
[AddonFeature(AddonEvent.PreFinalize)]
internal class SelectString : TextMatchingFeature
{
    protected override unsafe string GetSetLastSeenText(AtkUnitBase* atk)
    {
        var addon = new AddonMaster.SelectString(atk);
        P.LastSeenListEntries = [.. addon.Entries.Select(x => (x.Index, x.Text))];
        return string.Join(", ", addon.Entries.Select(x => x.Text));
    }

    protected override unsafe object? ShouldProceed(string text, AtkUnitBase* atk)
    {
        if (!GenericHelpers.TryGetAddonMaster<AddonMaster.SelectString>(out var addon)) return null;
        string[] entries = [.. addon.Entries.Select(x => x.Text)];

        var nodes = C.GetAllNodes().OfType<ListEntryNode>();
        foreach (var node in nodes)
        {
            if (!node.Enabled || string.IsNullOrEmpty(node.Text))
                continue;

            if (!CheckRestrictions(node))
                continue;

            var index = GetMatchingIndex(entries, node.Text, node.IsTextRegex);
            if (index.HasValue)
                return index.Value;
        }

        return null;
    }

    protected override unsafe void Proceed(AtkUnitBase* atk, object? matchingNode)
    {
        if (matchingNode is not int index) return;
        new AddonMaster.SelectString(atk).Entries[index].Select();
    }
}
