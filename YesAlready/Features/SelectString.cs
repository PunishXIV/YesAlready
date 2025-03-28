using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using System.Linq;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class SelectString : OnSetupSelectListFeature
{
    public override void Enable()
    {
        base.Enable();
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "SelectString", AddonSetup);
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PreFinalize, "SelectString", SetEntry);
    }

    private void SetEntry(AddonEvent type, AddonArgs args)
    {
        try
        {
            P.LastSeenListSelection = P.LastSeenListIndex < P.LastSeenListEntries.Length ? P.LastSeenListEntries?[P.LastSeenListIndex].Text ?? string.Empty : string.Empty;
            P.LastSeenListTarget = P.LastSeenListTarget = Svc.Targets.Target != null ? Svc.Targets.Target.Name.GetText() ?? string.Empty : string.Empty;
        }
        catch { }
    }

    public override void Disable()
    {
        base.Disable();
        Svc.AddonLifecycle.UnregisterListener(AddonSetup);
        Svc.AddonLifecycle.UnregisterListener(SetEntry);
    }

    protected unsafe void AddonSetup(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active) return;

        var addon = new AddonMaster.SelectString(addonInfo.Base());
        P.LastSeenListEntries = addon.Entries.Select(x => (x.Index, x.Text)).ToArray();

        var index = GetMatchingIndex(addon.Entries.Select(x => x.Text).ToArray());
        if (index != null)
            addon.Entries[(int)index].Select();
    }
}
