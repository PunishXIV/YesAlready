using System.Linq;

namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class SelectOk : TextMatchingFeature
{
    protected override unsafe string GetSetLastSeenText(AtkUnitBase* atk)
    {
        var text = new AddonMaster.SelectOk(atk).Text;
        P.LastSeenOkText = text;
        return text;
    }

    protected override unsafe object? ShouldProceed(string text, AtkUnitBase* atk)
    {
        var nodes = C.GetAllNodes().OfType<OkEntryNode>();
        foreach (var node in nodes)
        {
            if (!node.Enabled || string.IsNullOrEmpty(node.Text))
                continue;

            if (EntryMatchesText(node.Text, text, node.IsTextRegex))
                return node;
        }
        return null;
    }

    protected override unsafe void Proceed(AtkUnitBase* atk, object? matchingNode) => new AddonMaster.SelectOk(atk).Ok();
}
