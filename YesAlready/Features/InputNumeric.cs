using System;
using System.Linq;

namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class InputNumeric : TextMatchingFeature
{
    protected override unsafe string GetSetLastSeenText(AtkUnitBase* atk)
    {
        var text = atk->AtkValues[6].String;
        Service.Watcher.LastSeenNumericsText = atk->AtkValues[6].String;
        return text;
    }

    protected override unsafe object? ShouldProceed(string text, AtkUnitBase* atk)
    {
        var nodes = C.GetAllNodes().OfType<NumericsEntryNode>();
        foreach (var node in nodes)
        {
            if (!node.Enabled || string.IsNullOrEmpty(node.Text))
                continue;

            if (EntryMatchesText(node.Text, text, node.IsTextRegex))
                return node;
        }

        return null;
    }

    protected override unsafe void Proceed(AtkUnitBase* atk, object? matchingNode)
    {
        if (matchingNode is not NumericsEntryNode node) return;

        var min = atk->AtkValues[2].UInt;
        var max = atk->AtkValues[3].UInt;

        Log("Selecting ok");
        var value = Math.Clamp(node.IsPercent ? (uint)Math.Ceiling(max * (node.Percentage / 100f)) : (uint)node.Quantity, min, max);
        Callback.Fire(atk, true, (int)value);
    }
}
