using ClickLib.Clicks;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;
using ECommons;
using ECommons.Automation;
using ECommons.DalamudServices;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Collections.Generic;
using System.Linq;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;
internal class AddonSatisfactionSupplyFeature : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, "SatisfactionSupply", AddonUpdate);
        Svc.Framework.Update += RequestFill;
        Svc.Framework.Update += RequestComplete;
    }

    public override void Disable()
    {
        base.Disable();
        AddonLifecycle.UnregisterListener(AddonUpdate);
        Svc.Framework.Update -= RequestFill;
        Svc.Framework.Update -= RequestComplete;
    }

    protected static unsafe void AddonUpdate(AddonEvent eventType, AddonArgs args)
    {
        if (!P.Active || !P.Config.CustomDeliveries)
            return;

        var addon = (AtkUnitBase*)args.Addon;
        if (!GenericHelpers.IsAddonReady(addon)) return;

        var atkValues = new[] { addon->AtkValues[22].Int, addon->AtkValues[31].Int, addon->AtkValues[40].Int };
        foreach (var (index, value) in atkValues.Select((value, index) => (index, value)))
            if (value != 0 && !GenericHelpers.TryGetAddonByName<AtkUnitBase>("Request", out var ptr))
                Callback.Fire(addon, false, 1, index);
    }

    private static List<int> SlotsFilled { get; set; } = [];
    private static unsafe void RequestFill(IFramework framework)
    {
        if (!P.Active || !P.Config.CustomDeliveries || !GenericHelpers.TryGetAddonByName<AddonRequest>("SatisfactionSupply", out var _))
            return;

        if (GenericHelpers.TryGetAddonByName<AddonRequest>("Request", out var addon) && GenericHelpers.IsAddonReady((AtkUnitBase*)addon))
        {
            for (var i = 1; i <= addon->EntryCount; i++)
            {
                if (SlotsFilled.Contains(addon->EntryCount))
                {
                    P.TaskManager.Abort();
                    return;
                }
                if (SlotsFilled.Contains(i)) return;
                var val = i;
                P.TaskManager.Enqueue(() => TryClickItem(addon, val));
            }
        }
        else
        {
            SlotsFilled.Clear();
            P.TaskManager.Abort();
        }
    }

    private static unsafe bool? TryClickItem(AddonRequest* addon, int i)
    {
        if (SlotsFilled.Contains(i)) return true;

        var contextMenu = (AtkUnitBase*)Svc.GameGui.GetAddonByName("ContextIconMenu", 1);

        if (contextMenu is null || !contextMenu->IsVisible)
        {
            var slot = i - 1;
            var unk = (44 * i) + (i - 1);

            Callback.Fire(&addon->AtkUnitBase, false, 2, slot, 0, 0);

            return false;
        }
        else
        {
            Callback.Fire(contextMenu, false, 0, 0, 1021003, 0, 0);
            Svc.Log.Debug($"Filled slot {i}");
            SlotsFilled.Add(i);
            return true;
        }
    }

    private static ulong RequestAllow;
    private static unsafe void RequestComplete(IFramework framework)
    {
        if (!P.Active || !P.Config.CustomDeliveries || !GenericHelpers.TryGetAddonByName<AddonRequest>("SatisfactionSupply", out var _))
            return;

        if (GenericHelpers.TryGetAddonByName<AddonRequest>("Request", out var request) && GenericHelpers.IsAddonReady(&request->AtkUnitBase))
        {
            if (RequestAllow == 0)
            {
                RequestAllow = Svc.PluginInterface.UiBuilder.FrameCount + 4;
            }
            if (Svc.PluginInterface.UiBuilder.FrameCount < RequestAllow) return;
            var questAddon = (AtkUnitBase*)request;
            if (questAddon->UldManager.NodeListCount <= 16) return;
            var buttonNode = (AtkComponentNode*)questAddon->UldManager.NodeList[4];
            if (buttonNode->Component->UldManager.NodeListCount <= 2) return;
            var textComponent = (AtkTextNode*)buttonNode->Component->UldManager.NodeList[2];
            if (textComponent->AtkResNode.Color.A != 255) return;
            for (var i = 16; i <= 12; i--)
            {
                if (((AtkComponentNode*)questAddon->UldManager.NodeList[i])->AtkResNode.IsVisible
                    && ((AtkComponentNode*)questAddon->UldManager.NodeList[i - 6])->AtkResNode.IsVisible) return;
            }
            if (request->HandOverButton != null && request->HandOverButton->IsEnabled)
            {
                if (EzThrottler.Throttle("Handin"))
                {
                    Svc.Log.Debug("Handing over request");
                    ClickRequest.Using((nint)request).HandOver();
                }
            }
        }
        else
        {
            RequestAllow = 0;
        }
    }
}
