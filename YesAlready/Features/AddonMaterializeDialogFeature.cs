using ClickLib.Clicks;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class AddonMaterializeDialogFeature : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "MaterializeDialog", AddonSetup);
    }

    public override void Disable()
    {
        base.Disable();
        AddonLifecycle.UnregisterListener(AddonSetup);
    }

    protected static unsafe void AddonSetup(AddonEvent eventType, AddonArgs addonInfo)
    {
        var addon = (AtkUnitBase*)addonInfo.Addon;

        if (!P.Config.Enabled || !P.Config.MaterializeDialogEnabled)
            return;

        ClickMaterializeDialog.Using((nint)addon).Materialize();
    }
}
