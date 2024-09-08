using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Linq;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class AddonTalkFeature : BaseFeature
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

    private string lastTalkTarget = string.Empty;
    private bool matched = false;

    protected unsafe void AddonUpdate(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active || !((AtkUnitBase*)addonInfo.Addon)->IsVisible) return;

        var addon = new AddonMaster.Talk(addonInfo.Base());
        var target = Svc.Targets.Target;
        var targetName = P.LastSeenTalkTarget = target != null
            ? Utils.SEString.GetSeStringText(target.Name)
            : string.Empty;

        if (P.ForcedYesKeyPressed && !P.Config.SeparateForcedKeys || P.ForcedTalkKeyPressed)
        {
            Svc.Log.Debug($"{nameof(AddonTalkFeature)}: Forced hotkey pressed");
            addon.Click();
            return;
        }

        if (targetName != lastTalkTarget)
        {
            Svc.Log.Debug("Target Name: " + targetName + ", lastTalkTarget: " + lastTalkTarget);
            lastTalkTarget = targetName;
            matched = false;

            var nodes = P.Config.GetAllNodes().OfType<TalkEntryNode>();

            foreach (var node in nodes)
            {
                if (!node.Enabled || string.IsNullOrEmpty(node.TargetText))
                    continue;

                matched = EntryMatchesTargetName(node, targetName);

                if (matched)
                {
                    Svc.Log.Debug("Talk match seen: " + matched + " Node: " + node.Name);
                    break;
                }
            }
        }

        if (matched)
        {
            Svc.Log.Debug("AddonTalk: Advancing");
            addon.Click();
            return;
        }
    }

    private static bool EntryMatchesTargetName(TalkEntryNode node, string targetName)
        => node.TargetIsRegex && (node.TargetRegex?.IsMatch(targetName) ?? false) || !node.TargetIsRegex && targetName.Contains(node.TargetText);
}
