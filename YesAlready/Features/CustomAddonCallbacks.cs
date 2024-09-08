using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons.Automation;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Linq;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;
public class CustomAddonCallbacks : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        foreach (var addon in P.Config.CustomBothers)
            Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, addon.Addon, AddonSetup);
    }

    public override void Disable()
    {
        base.Disable();
        Svc.AddonLifecycle.UnregisterListener(AddonSetup);
    }

    protected static unsafe void AddonSetup(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active) return;

        var addon = (AtkUnitBase*)addonInfo.Addon;
        var callbacks = P.Config.CustomBothers.First(x => x.Addon == addonInfo.AddonName).CallbackParams;
        Callback.Fire(addon, true, callbacks);
    }
}
