using System.Linq;

namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostUpdate)]
internal class Talk : TextMatchingFeature
{
    protected override unsafe string GetSetLastSeenText(AtkUnitBase* atk)
    {
        var text = Svc.Targets.Target is { Name: var name } ? Utils.SEString.GetSeStringText(name) : string.Empty;
        P.LastSeenTalkTarget = text;
        return text;
    }

    protected override unsafe object? ShouldProceed(string text, AtkUnitBase* atk)
    {
        if (P.ForcedYesKeyPressed && !P.Config.SeparateForcedKeys || P.ForcedTalkKeyPressed)
        {
            PluginLog.Debug($"{nameof(Talk)}: Forced hotkey pressed");
            return true;
        }

        var nodes = P.Config.GetAllNodes().OfType<TalkEntryNode>();
        foreach (var node in nodes)
        {
            if (!node.Enabled || string.IsNullOrEmpty(node.TargetText))
                continue;

            if (EntryMatchesText(node.TargetText, text, node.TargetIsRegex))
                return node;
        }

        return null;
    }

    protected override unsafe void Proceed(AtkUnitBase* atk, object? matchingNode)
    {
        if (GenericHelpers.TryGetAddonMaster<AddonMaster.Talk>(out var addon))
            addon.Click();
    }
}
