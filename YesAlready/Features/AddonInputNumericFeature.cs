using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons.Automation;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Linq;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;
internal class AddonInputNumericFeature : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "InputNumeric", AddonSetup);
    }

    public override void Disable()
    {
        base.Disable();
        Svc.AddonLifecycle.UnregisterListener(AddonSetup);
    }

    protected static unsafe void AddonSetup(AddonEvent eventType, AddonArgs args)
    {
        if (!P.Active)
            return;

        var addon = (AtkUnitBase*)args.Addon;
        var min = addon->AtkValues[2].UInt;
        var max = addon->AtkValues[3].UInt;
        var text = P.LastSeenNumericsText = Utils.SEString.GetSeStringText(new nint(addon->AtkValues[6].String));
        Svc.Log.Debug($"AddonInputNumeric: text={text}");

        var nodes = P.Config.GetAllNodes().OfType<NumericsEntryNode>();
        foreach (var node in nodes)
        {
            if (!node.Enabled || string.IsNullOrEmpty(node.Text))
                continue;
            if (!EntryMatchesText(node, text))
                continue;

            Svc.Log.Debug("AddonInputNumeric: Selecting ok");
            var value = Math.Clamp(node.IsPercent ? (uint)Math.Ceiling(max * (node.Percentage / 100f)) : (uint)node.Quantity, min, max);
            Callback.Fire(addon, true, (int)value);
            return;
        }
    }
    private static bool EntryMatchesText(NumericsEntryNode node, string text)
        => node.IsTextRegex && (node.TextRegex?.IsMatch(text) ?? false) || !node.IsTextRegex && text.Contains(node.Text);
}
