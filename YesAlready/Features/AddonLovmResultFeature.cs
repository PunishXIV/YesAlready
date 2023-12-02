using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons.Automation;
using FFXIVClientStructs.FFXIV.Component.GUI;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class AddonLovmResultFeature : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "LovmResult", AddonSetup);
    }

    public override void Disable() {
        base.Disable();
        AddonLifecycle.UnregisterListener(AddonSetup);
    }

    protected static unsafe void AddonSetup(AddonEvent eventType, AddonArgs addonInfo)
    {
        var addon = (AtkUnitBase*)addonInfo.Addon;

        if (!P.Active || !P.Config.LordOfVerminionQuit)
            return;

        Callback.Fire(addon, true, -1);
    }
}
