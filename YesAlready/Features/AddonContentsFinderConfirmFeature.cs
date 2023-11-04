using ClickLib.Clicks;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class AddonContentsFinderConfirmFeature : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "ContentsFinderConfirm", AddonSetup);
    }

    public override void Disable() {
        base.Disable();
        AddonLifecycle.UnregisterListener(AddonSetup);
    }

    protected static unsafe void AddonSetup(AddonEvent eventType, AddonArgs addonInfo)
    {
        var addon = (AtkUnitBase*)addonInfo.Addon;

        if (!P.Config.Enabled || !P.Config.ContentsFinderConfirmEnabled)
            return;

        ClickContentsFinderConfirm.Using((nint)addon).Commence();

        if (P.Config.ContentsFinderOneTimeConfirmEnabled)
        {
            P.Config.ContentsFinderConfirmEnabled = false;
            P.Config.ContentsFinderOneTimeConfirmEnabled = false;
            P.Config.Save();
        }
    }
}
