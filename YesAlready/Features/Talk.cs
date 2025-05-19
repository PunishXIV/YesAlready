using System.Linq;

namespace YesAlready.Features;

internal class Talk : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, "Talk", AddonUpdate);
    }

    public override void Disable()
    {
        base.Disable();
        Svc.AddonLifecycle.UnregisterListener(AddonUpdate);
    }

    protected unsafe void AddonUpdate(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active) return;

        if (!GenericHelpers.TryGetAddonMaster<AddonMaster.Talk>(out var addon) || !addon.IsAddonReady) return;
        var target = Svc.Targets.Target;
        var targetName = P.LastSeenTalkTarget = target != null
            ? Utils.SEString.GetSeStringText(target.Name)
            : string.Empty;

        if (P.ForcedYesKeyPressed && !P.Config.SeparateForcedKeys || P.ForcedTalkKeyPressed)
        {
            PluginLog.Debug($"{nameof(Talk)}: Forced hotkey pressed");
            addon.Click();
            return;
        }

        // if someone sees this and thinks "This is unoptimal, what if I have a lot of regex in my talk entries?", I will simply say "why would you have that?"
        // knock yourself out optimising this without changing any current functionality
        var nodes = P.Config.GetAllNodes().OfType<TalkEntryNode>();
        foreach (var node in nodes)
        {
            if (!node.Enabled || string.IsNullOrEmpty(node.TargetText)) continue;
            var matched = EntryMatchesTargetName(node, targetName);
            if (matched)
            {
                PluginLog.Debug($"AddonTalk: Matched {targetName} to {node.Name}. Advancing");
                addon.Click();
                return;
            }
        }
    }

    private static bool EntryMatchesTargetName(TalkEntryNode node, string targetName)
        => node.TargetIsRegex && (node.TargetRegex?.IsMatch(targetName) ?? false) || !node.TargetIsRegex && targetName.Contains(node.TargetText);
}
