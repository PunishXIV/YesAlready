using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;
using ECommons.Automation;
using ECommons.Logging;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Collections.Generic;
using System.Linq;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;
internal class SatisfactionSupply : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, "SatisfactionSupply", AddonUpdate);
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PreSetup, "SatisfactionSupply", Reset);
        Svc.Framework.Update += RequestFill;
        Svc.Framework.Update += RequestComplete;
    }


    public override void Disable()
    {
        base.Disable();
        Svc.AddonLifecycle.UnregisterListener(AddonUpdate);
        Svc.AddonLifecycle.UnregisterListener(Reset);
        Svc.Framework.Update -= RequestFill;
        Svc.Framework.Update -= RequestComplete;
    }

    private static bool Disabled;
    protected static unsafe void AddonUpdate(AddonEvent eventType, AddonArgs args)
    {
        if (!P.Active || !P.Config.CustomDeliveries || Disabled)
            return;

        var addon = args.Base();
        if (!GenericHelpers.IsAddonReady(addon)) return;

        var atkValues = new[] { addon->AtkValues[22].Int, addon->AtkValues[31].Int, addon->AtkValues[40].Int };
        foreach (var (index, value) in atkValues.Select((value, index) => (index, value)))
        {
            if (value != 0 && !GenericHelpers.TryGetAddonByName<AtkUnitBase>("Request", out var _))
            {
                if (WillOvercap(addon, index))
                {
                    Utils.SEString.PrintPluginMessage("Further turn in will overcap scrips.");
                    Disabled = true;
                    return;
                }
                Callback.Fire(addon, false, 1, index);
            }
        }
    }

    private void Reset(AddonEvent type, AddonArgs args) => Disabled = false;

    private static unsafe bool WillOvercap(AtkUnitBase* addon, int row)
    {
        var agent = AgentSatisfactionSupply.Instance();
        if (agent == null) return true;
        var item = agent->Items[row];
        var invItem = FindItemInInventory(item.Id);
        var invItemColectability = InventoryManager.Instance()->GetInventoryContainer(invItem.Value.inv)->GetInventorySlot(invItem.Value.slot)->Spiritbond;
        var wc = InventoryManager.Instance()->GetInventoryItemCount(AgentSatisfactionSupply.Instance()->CrafterScripId1);
        var pc = InventoryManager.Instance()->GetInventoryItemCount(AgentSatisfactionSupply.Instance()->CrafterScripId2);
        var wg = InventoryManager.Instance()->GetInventoryItemCount(AgentSatisfactionSupply.Instance()->GathererScripId1);
        var pg = InventoryManager.Instance()->GetInventoryItemCount(AgentSatisfactionSupply.Instance()->GathererScripId2);

        // this is awful
        if (invItemColectability >= item.Collectability3)
        {
            switch (row)
            {
                case 0:
                    if (wc + addon->AtkValues[61].Int > 4000 || pc + addon->AtkValues[64].Int > 4000)
                        return true;
                    break;
                case 1:
                    if (wg + addon->AtkValues[89].Int > 4000 || pg + addon->AtkValues[92].Int > 4000)
                        return true;
                    break;
                case 2:
                    if (wg + addon->AtkValues[117].Int > 4000 || pg + addon->AtkValues[120].Int > 4000)
                        return true;
                    break;
            }
        }
        if (invItemColectability >= item.Collectability2)
        {
            switch (row)
            {
                case 0:
                    if (wc + addon->AtkValues[60].Int > 4000 || pc + addon->AtkValues[63].Int > 4000)
                        return true;
                    break;
                case 1:
                    if (wg + addon->AtkValues[88].Int > 4000 || pg + addon->AtkValues[91].Int > 4000)
                        return true;
                    break;
                case 2:
                    if (wg + addon->AtkValues[116].Int > 4000 || pg + addon->AtkValues[119].Int > 4000)
                        return true;
                    break;
            }
        }
        if (invItemColectability >= item.Collectability1)
        {
            switch (row)
            {
                case 0:
                    if (wc + addon->AtkValues[59].Int > 4000 || pc + addon->AtkValues[62].Int > 4000)
                        return true;
                    break;
                case 1:
                    if (wg + addon->AtkValues[87].Int > 4000 || pg + addon->AtkValues[90].Int > 4000)
                        return true;
                    break;
                case 2:
                    if (wg + addon->AtkValues[115].Int > 4000 || pg + addon->AtkValues[118].Int > 4000)
                        return true;
                    break;
            }
        }
        return false;
    }

    private static unsafe (InventoryType inv, int slot)? FindItemInInventory(uint itemId)
    {
        IEnumerable<InventoryType> x = [InventoryType.Inventory1, InventoryType.Inventory2, InventoryType.Inventory3, InventoryType.Inventory4];
        foreach (var inv in x)
        {
            var cont = InventoryManager.Instance()->GetInventoryContainer(inv);
            for (var i = 0; i < cont->Size; ++i)
            {
                if (cont->GetInventorySlot(i)->ItemId == itemId)
                {
                    return (inv, i);
                }
            }
        }
        return null;
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
            var unk = 44 * i + (i - 1);

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

        if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("Request", out var addon) && GenericHelpers.IsAddonReady(addon))
        {
            if (RequestAllow == 0)
                RequestAllow = Svc.PluginInterface.UiBuilder.FrameCount + 4;

            if (Svc.PluginInterface.UiBuilder.FrameCount < RequestAllow) return;
            var m = new AddonMaster.Request(addon);
            if (m.IsHandOverEnabled && m.IsFilled)
            {
                if (EzThrottler.Throttle("Handin"))
                {
                    PluginLog.Debug("Handing over request");
                    m.HandOver();
                }
            }
        }
        else
            RequestAllow = 0;
    }
}
