using System;

using ClickLib.Clicks;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class AddonSelectIconStringFeature : OnSetupSelectListFeature
{
    public override void Enable()
    {
        base.Enable();
        AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "SelectIconString", AddonSetup);
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

        var addonPtr = (AddonSelectIconString*)addon;
        var popupMenu = &addonPtr->PopupMenu.PopupMenu;

        SetupOnItemSelectedHook(popupMenu);
        CompareNodesToEntryTexts((nint)addon, popupMenu);
    }

    protected override void SelectItemExecute(IntPtr addon, int index)
    {
        Svc.Log.Debug($"AddonSelectIconString: Selecting {index}");
        ClickSelectIconString.Using(addon).SelectItem((ushort)index);
    }
}
