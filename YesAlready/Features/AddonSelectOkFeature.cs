using System.Linq;

using ClickLib.Clicks;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Component.GUI;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class AddonSelectOkFeature : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "SelectOk", AddonSetup);
    }

    public override void Disable()
    {
        base.Disable();
        AddonLifecycle.UnregisterListener(AddonSetup);
    }

    protected unsafe void AddonSetup(AddonEvent eventType, AddonArgs addonInfo)
    {
        var addon = (AtkUnitBase*)addonInfo.Addon;

        if (!P.Active)
            return;

        var text = P.LastSeenOkText = Utils.SEString.GetSeStringText(new nint(addon->AtkValues[0].String));
        Svc.Log.Debug($"AddonSelectOk: text={text}");

        var nodes = P.Config.GetAllNodes().OfType<OkEntryNode>();
        foreach (var node in nodes)
        {
            if (!node.Enabled || string.IsNullOrEmpty(node.Text))
                continue;

            if (!EntryMatchesText(node, text))
                continue;

            Svc.Log.Debug("AddonSelectOk: Selecting ok");
            ClickSelectOk.Using(new nint(addon)).Ok();
            return;
        }
    }

    private static bool EntryMatchesText(OkEntryNode node, string text)
    {
        return (node.IsTextRegex && (node.TextRegex?.IsMatch(text) ?? false)) ||
              (!node.IsTextRegex && text.Contains(node.Text));
    }
}
