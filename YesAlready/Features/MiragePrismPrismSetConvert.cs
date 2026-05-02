using ECommons.UIHelpers;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using System.Collections.Generic;
using System.Linq;

namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostRefresh)]
public class MiragePrismPrismSetConvert : AddonFeature
{
    protected override bool IsEnabled() => C.MiragePrismPrismSetConvert;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk)
    {
        if (!GenericHelpers.IsAddonReady(atk)) return;

        if (GenericHelpers.TryGetAddonMaster<CustomAddonMaster.MiragePrismPrismSetConvert>(out var am))
        {
            if (am.Addon->GetNodeById(12)->IsVisible())
            {
                Svc.Chat.PrintPluginMessage($"Outfit already in dresser");
                return;
            }

            if (am.Items.Any(i => i.Flag is CustomAddonMaster.ReaderMiragePrismPrismSetConvert.ItemFlag.Missing) && !C.AllowPartialFilling) return;
            foreach (var (item, i) in am.Items.WithIndex())
            {
                if (item.Flag is not CustomAddonMaster.ReaderMiragePrismPrismSetConvert.ItemFlag.Unfilled)
                    continue;
                var s = i;
                Service.TaskManager.Enqueue(() => am.TryHandOver(s), $"HandInSlot{s}");
                Service.TaskManager.Enqueue(() => item.Flag is CustomAddonMaster.ReaderMiragePrismPrismSetConvert.ItemFlag.Filled);
            }
            Service.TaskManager.Enqueue(am.StoreAsGlamour);
        }
    }

    public class CustomAddonMaster
    {
        public unsafe class MiragePrismPrismSetConvert : AddonMasterBase<AtkUnitBase>
        {
            public MiragePrismPrismSetConvert(nint addon) : base(addon) { }
            public MiragePrismPrismSetConvert(void* addon) : base(addon) { }
            public override string AddonDescription { get; } = "Outfit glamour creation";

            public bool IsNewOutfit => Addon->Id == 130;
            public bool IsExistingOutfit => Addon->Id == 131;

            public AtkComponentButton* StoreAsGlamourButton => Addon->GetComponentButtonById(27);
            public AtkComponentButton* CloseButton => Addon->GetComponentButtonById(26);

            public void StoreAsGlamour() => ClickButtonIfEnabled(StoreAsGlamourButton);
            public void Close() => ClickButtonIfEnabled(CloseButton);

            public class Item(ReaderMiragePrismPrismSetConvert.Item handle) : ReaderMiragePrismPrismSetConvert.Item(handle.AtkReaderParams.UnitBase, handle.AtkReaderParams.BeginOffset);
            public Item[] Items
            {
                get
                {
                    var reader = new ReaderMiragePrismPrismSetConvert(Base);
                    var entries = new Item[reader.Items.Count];
                    for (var i = 0; i < entries.Length; i++)
                        entries[i] = new(reader.Items[i]);
                    return entries;
                }
            }
            public uint GlamourPrismsHeld => new ReaderMiragePrismPrismSetConvert(Base).GlamourPrismsHeld;
            public uint GlamourPrismsRequired => new ReaderMiragePrismPrismSetConvert(Base).ItemCount;

            public List<int> SlotsFilled => Enumerable.Range(0, Items.Length).Where(x => Items[x].Flag is ReaderMiragePrismPrismSetConvert.ItemFlag.Filled or ReaderMiragePrismPrismSetConvert.ItemFlag.AlreadyInOutfit).ToList();
            public int? FirstUnfilledSlot => SlotsFilled.Count == Items.Length ? null : Enumerable.Range(0, Items.Length).FirstOrDefault(i => !SlotsFilled.Contains(i));

            public bool? TryHandOver(int slot)
            {
                if (SlotsFilled.Contains(slot)) return true;

                var contextMenu = (AtkUnitBase*)Svc.GameGui.GetAddonByName("ContextIconMenu", 1).Address;

                if (contextMenu is null || !contextMenu->IsVisible)
                {
                    Callback.Fire(Base, true, 13, slot);
                    return false;
                }
                else
                {
                    Callback.Fire(contextMenu, true, 0, 0, Items[slot].ItemIconId, 0u, 0);
                    PluginLog.Debug($"Filled slot {slot}");
                    return true;
                }
            }
        }

        public unsafe class ReaderMiragePrismPrismSetConvert(AtkUnitBase* Addon) : AtkReader(Addon)
        {
            public uint Unk00 => ReadUInt(0) ?? 0;
            public uint GlamourPrismsHeld => ReadUInt(1) ?? 0;
            public uint Unk02 => ReadUInt(2) ?? 0;
            public uint Unk03 => ReadUInt(3) ?? 0;
            public uint Unk04 => ReadUInt(4) ?? 0;
            public uint OutfitIconId => ReadUInt(5) ?? 0;
            /// <remarks>
            /// Also the amount of glamour prisms required
            /// </remarks>
            public uint ItemCount => AgentMiragePrismPrismSetConvert.Instance()->Data->NumItemsInSet;
            public List<Item> Items => Loop<Item>(21, 7, (int)ItemCount);

            public unsafe class Item(nint Addon, int start) : AtkReader(Addon, start)
            {
                public uint ItemId => ReadUInt(0) ?? 0;
                public uint ItemIconId => ReadUInt(1) ?? 0;
                public uint Unk03 => ReadUInt(2) ?? 0;
                public uint Unk04 => ReadUInt(3) ?? 0;
                public uint InventoryType => ReadUInt(4) ?? 0; // 9999 if the item hasn't been filled
                public uint InventorySlot => ReadUInt(5) ?? 0; // 0 if item hasn't been filled
                public ItemFlag Flag => (ItemFlag)(ReadUInt(6) ?? 0);
            }

            public enum ItemFlag : uint
            {
                Missing = 0,
                Unfilled = 2,
                Filled = 3,
                AlreadyInOutfit = 6
            }
        }
    }
}
