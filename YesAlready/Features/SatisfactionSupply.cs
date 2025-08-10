using Dalamud.Game.Inventory;
using Dalamud.Plugin.Services;
using ECommons.Throttlers;
using ECommons.UIHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using System;
using System.Collections.Generic;

namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostUpdate)]
internal class SatisfactionSupply : AddonFeature
{
    protected override bool IsEnabled() => C.CustomDeliveries;

    private static bool Disabled;
    private static List<int> SlotsFilled { get; set; } = [];
    private static ulong RequestAllow;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk)
    {
        if (Disabled || !GenericHelpers.IsAddonReady(atk)) return;
        var reader = new ReaderSatisfactionSupply(atk);

        foreach (var (value, index) in reader.Quantities.WithIndex())
        {
            if (value != 0 && !GenericHelpers.TryGetAddonByName<AtkUnitBase>("Request", out var _))
            {
                if (reader.WillItemOvercap(AgentSatisfactionSupply.Instance()->Items[index], Log))
                {
                    Svc.Chat.PrintPluginMessage("Further turn in will overcap scrips.");
                    Disabled = true;
                    return;
                }
                Log($"Turning in item #{AgentSatisfactionSupply.Instance()->Items[index].Id}");
                Callback.Fire(atk, false, 1, index);
            }
        }
    }

    public override void Enable()
    {
        base.Enable();
        Svc.Framework.Update += RequestFill;
        Svc.Framework.Update += RequestComplete;
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PreSetup, "SatisfactionSupply", Reset);
    }

    public override void Disable()
    {
        base.Disable();
        Svc.Framework.Update -= RequestFill;
        Svc.Framework.Update -= RequestComplete;
        Svc.AddonLifecycle.UnregisterListener(Reset);
    }

    private void Reset(AddonEvent type, AddonArgs args) => Disabled = false;

    private static unsafe void RequestFill(IFramework framework)
    {
        if (!P.Active || !C.CustomDeliveries || !GenericHelpers.TryGetAddonByName<AddonRequest>("SatisfactionSupply", out var _))
            return;

        if (GenericHelpers.TryGetAddonByName<AddonRequest>("Request", out var addon) && GenericHelpers.IsAddonReady((AtkUnitBase*)addon))
        {
            for (var i = 1; i <= addon->EntryCount; i++)
            {
                if (SlotsFilled.Contains(addon->EntryCount))
                {
                    Service.TaskManager.Abort();
                    return;
                }
                if (SlotsFilled.Contains(i)) return;
                var val = i;
                Service.TaskManager.Enqueue(() => TryClickItem(addon, val));
            }
        }
        else
        {
            SlotsFilled.Clear();
            Service.TaskManager.Abort();
        }
    }

    private static unsafe bool? TryClickItem(AddonRequest* addon, int i)
    {
        if (SlotsFilled.Contains(i)) return true;

        var contextMenu = (AtkUnitBase*)Svc.GameGui.GetAddonByName("ContextIconMenu", 1).Address;

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
            PluginLog.Debug($"Filled slot {i}");
            SlotsFilled.Add(i);
            return true;
        }
    }

    private static unsafe void RequestComplete(IFramework framework)
    {
        if (!P.Active || !C.CustomDeliveries || !GenericHelpers.TryGetAddonByName<AddonRequest>("SatisfactionSupply", out var _))
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

public unsafe class ReaderSatisfactionSupply(AtkUnitBase* UnitBase, int BeginOffset = 0) : AtkReader(UnitBase, BeginOffset)
{
    public List<int> Quantities => [DoHQuantity, MinBotQuantity, FshQuantity];
    public int DoHQuantity => ReadInt(22) ?? 0;
    public int MinBotQuantity => ReadInt(31) ?? 0;
    public int FshQuantity => ReadInt(40) ?? 0;

    public AgentSatisfactionSupply.ItemInfo DoHItem => AgentSatisfactionSupply.Instance()->Items[0];
    public AgentSatisfactionSupply.ItemInfo MinBotItem => AgentSatisfactionSupply.Instance()->Items[1];
    public AgentSatisfactionSupply.ItemInfo FshItem => AgentSatisfactionSupply.Instance()->Items[2];

    public Span<uint> CraftScripIds => AgentSatisfactionSupply.Instance()->CrafterScripIds;
    public Span<uint> GatherScripIds => AgentSatisfactionSupply.Instance()->GathererScripIds;

    public bool WillItemOvercap(AgentSatisfactionSupply.ItemInfo item, Action<string> log)
    {
        if (GetItem(item.Id) is { SpiritbondOrCollectability: var collectability })
        {
            log($"Checking overcap for item #{item.Id} with collectability {collectability}");
            if (collectability > item.Collectability3)
            {
                log($"Item #{item.Id} [{item.Reward1Quantity[2]} > {CurrencyManager.Instance()->GetItemCountRemaining(item.Reward1Id)} || {item.Reward2Quantity[2]} > {CurrencyManager.Instance()->GetItemCountRemaining(item.Reward2Id)}]");
                return CurrencyManager.Instance()->GetItemCountRemaining(item.Reward1Id) < item.Reward1Quantity[2] || CurrencyManager.Instance()->GetItemCountRemaining(item.Reward2Id) < item.Reward2Quantity[2];
            }
            if (collectability > item.Collectability2)
            {
                log($"Item #{item.Id} [{item.Reward1Quantity[1]} > {CurrencyManager.Instance()->GetItemCountRemaining(item.Reward1Id)} || {item.Reward2Quantity[1]} > {CurrencyManager.Instance()->GetItemCountRemaining(item.Reward2Id)}]");
                return CurrencyManager.Instance()->GetItemCountRemaining(item.Reward1Id) < item.Reward1Quantity[1] || CurrencyManager.Instance()->GetItemCountRemaining(item.Reward2Id) < item.Reward2Quantity[1];
            }
            if (collectability > item.Collectability1)
            {
                log($"Item #{item.Id} [{item.Reward1Quantity[0]} > {CurrencyManager.Instance()->GetItemCountRemaining(item.Reward1Id)} || {item.Reward2Quantity[0]} > {CurrencyManager.Instance()->GetItemCountRemaining(item.Reward2Id)}]");
                return CurrencyManager.Instance()->GetItemCountRemaining(item.Reward1Id) < item.Reward1Quantity[0] || CurrencyManager.Instance()->GetItemCountRemaining(item.Reward2Id) < item.Reward2Quantity[0];
            }
        }
        throw new Exception($"Failed to find item [{item.Id}] in inventory");
    }

    public List<CollectabilityReward> DoHRewards => Loop<CollectabilityReward>(59, 1, 6);
    public List<CollectabilityReward> MinBotRewards => Loop<CollectabilityReward>(87, 1, 6);
    public List<CollectabilityReward> FshRewards => Loop<CollectabilityReward>(115, 1, 6);
    public class CollectabilityReward(nint UnitBasePtr, int BeginOffset = 0) : AtkReader(UnitBasePtr, BeginOffset)
    {
        public uint Scrip1LowCollectability => ReadUInt(0) ?? 0;
        public uint Scrip1MedCollectability => ReadUInt(1) ?? 0;
        public uint Scrip1HighCollectability => ReadUInt(2) ?? 0;
        public uint Scrip2LowCollectability => ReadUInt(3) ?? 0;
        public uint Scrip2MedCollectability => ReadUInt(4) ?? 0;
        public uint Scrip2HighCollectability => ReadUInt(5) ?? 0;
    }

    private GameInventoryItem? GetItem(uint itemId)
    {
        IEnumerable<GameInventoryType> types = [GameInventoryType.Inventory1, GameInventoryType.Inventory2, GameInventoryType.Inventory3, GameInventoryType.Inventory4];
        foreach (var type in types)
        {
            var items = Svc.GameInventory.GetInventoryItems(type);
            foreach (var item in items)
                if (item.BaseItemId == itemId)
                    return item;
        }
        return null;
    }
}
