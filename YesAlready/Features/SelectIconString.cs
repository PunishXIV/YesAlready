using System.Linq;

namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
[AddonFeature(AddonEvent.PreFinalize)]
internal class SelectIconString : TextMatchingFeature
{
    protected override unsafe string GetSetLastSeenText(AtkUnitBase* atk)
    {
        var addon = new AddonMaster.SelectIconString(atk);
        P.LastSeenListEntries = [.. addon.Entries.Select(x => (x.Index, x.Text))];
        return string.Join(", ", addon.Entries.Select(x => x.Text));
    }

    protected override unsafe object? ShouldProceed(string text, AtkUnitBase* atk)
    {
        if (!GenericHelpers.TryGetAddonMaster<AddonMaster.SelectIconString>(out var addon)) return null;
        string[] entries = [.. addon.Entries.Select(x => x.Text)];
        return GetMatchingIndex(entries);
    }

    protected override unsafe void Proceed(AtkUnitBase* atk, object? matchingNode)
    {
        if (matchingNode is not int index) return;
        var addon = new AddonMaster.SelectIconString(atk);
        addon.Entries[index].Select();
    }
}
