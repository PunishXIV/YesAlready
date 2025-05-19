using System.Linq;

namespace YesAlready.Features;

internal class SelectOk : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "SelectOk", AddonSetup);
    }

    public override void Disable()
    {
        base.Disable();
        Svc.AddonLifecycle.UnregisterListener(AddonSetup);
    }

    protected unsafe void AddonSetup(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active) return;

        var addon = new AddonMaster.SelectOk(addonInfo.Base());
        var text = P.LastSeenOkText = addon.Text;
        PluginLog.Debug($"AddonSelectOk: text={text}");

        var nodes = P.Config.GetAllNodes().OfType<OkEntryNode>();
        foreach (var node in nodes)
        {
            if (!node.Enabled || string.IsNullOrEmpty(node.Text))
                continue;

            if (!EntryMatchesText(node, text))
                continue;

            PluginLog.Debug("AddonSelectOk: Selecting ok");
            addon.Ok();
            return;
        }
    }

    private static bool EntryMatchesText(OkEntryNode node, string text)
        => node.IsTextRegex && (node.TextRegex?.IsMatch(text) ?? false) || !node.IsTextRegex && text.Contains(node.Text);
}
