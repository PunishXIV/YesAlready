using System;
using System.Linq;

using ClickLib.Clicks;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class AddonTalkFeature : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, "Talk", AddonUpdate);
    }

    public override void Disable()
    {
        base.Disable();
        AddonLifecycle.UnregisterListener(AddonUpdate);
    }

    private ClickTalk? clickTalk = null;
    private IntPtr lastTalkAddon = IntPtr.Zero;
    // private string lastTalkTarget = string.Empty;
    private bool matched = false;

    protected unsafe void AddonUpdate(AddonEvent eventType, AddonArgs addonInfo)
    {
        var addon = (AtkUnitBase*)addonInfo.Addon;

        if (!P.Active || addon == null || !GenericHelpers.IsAddonReady(addon))
            return;

        if ((P.ForcedYesKeyPressed && !P.Config.SeparateForcedKeys) || P.ForcedTalkKeyPressed)
        {
            Svc.Log.Debug($"{nameof(AddonTalkFeature)}: Forced hotkey pressed");
            if (clickTalk == null || lastTalkAddon != (IntPtr)addon)
                clickTalk = ClickTalk.Using(lastTalkAddon = (IntPtr)addon);
            clickTalk.Click();
            return;
        }

        var target = Svc.Targets.Target;
        var targetName = target != null
            ? Utils.SEString.GetSeStringText(target.Name)
            : string.Empty;

        if (targetName != P.LastSeenTalkTarget) {
            Svc.Log.Debug("Target Name: " + targetName + ", lastTalkTarget: " + P.LastSeenTalkTarget);
            P.LastSeenTalkTarget = targetName;
            matched = false;

            var nodes = P.Config.GetAllNodes().OfType<TalkEntryNode>();

            foreach (var node in nodes)
            {
                if (!node.Enabled || string.IsNullOrEmpty(node.TargetText))
                    continue;
                
                matched = EntryMatchesTargetName(node, targetName);

                if (matched) {
                    Svc.Log.Debug("Talk match seen: " + matched + " Node: " + node.Name);
                    break;    
                }
            }
        }

        if ( matched ) {
            Svc.Log.Debug("AddonTalk: Advancing");
            if (clickTalk == null || lastTalkAddon != (IntPtr)addon)
                clickTalk = ClickTalk.Using(lastTalkAddon = (IntPtr)addon);
            clickTalk.Click();
            return;
        }
    }

    private static bool EntryMatchesTargetName(TalkEntryNode node, string targetName)
    {
        return (node.TargetIsRegex && (node.TargetRegex?.IsMatch(targetName) ?? false)) ||
              (!node.TargetIsRegex && targetName.Contains(node.TargetText));
    }
}
