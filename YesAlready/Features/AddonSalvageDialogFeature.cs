using ClickLib.Clicks;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class AddonSalvageDialogFeature : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "SalvageDialog", AddonSetup);
    }

    public override void Disable()
    {
        base.Disable();
        AddonLifecycle.UnregisterListener(AddonSetup);
    }

    protected unsafe void AddonSetup(AddonEvent eventType, AddonArgs addonInfo)
    {
        var addon = (AtkUnitBase*)addonInfo.Addon;

        if (!P.Config.Enabled)
            return;

        if (P.Config.DesynthBulkDialogEnabled)
        {
            ((AddonSalvageDialog*)addon)->BulkDesynthEnabled = true;
        }

        if (P.Config.DesynthDialogEnabled)
        {
            var clickAddon = ClickSalvageDialog.Using((nint)addon);
            clickAddon.CheckBox();
            clickAddon.Desynthesize();
        }
    }
}
