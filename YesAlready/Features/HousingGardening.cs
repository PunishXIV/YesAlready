using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.Sheets;

namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup, "HousingGardening")]
[AddonFeature(AddonEvent.PreUpdate, "HousingGardening")]
[AddonFeature(AddonEvent.PostShow, "Inventory")]
[AddonFeature(AddonEvent.PostShow, "InventoryLarge")]
[AddonFeature(AddonEvent.PostShow, "InventoryExpansion")]
[Bother(nameof(Configuration.HousingGardeningAutoSelect), BotherCategory.Other, "Automatically select soil and seed in the gardening window.")]
internal unsafe class HousingGardening : AddonFeature
{
    private bool _handled;

    protected override void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo)
    {
        switch (addonInfo.AddonName)
        {
            case "HousingGardening":
                switch (eventType)
                {
                    case AddonEvent.PostSetup:
                        _handled = false;
                        return;
                    case AddonEvent.PreUpdate:
                        TrySelectSoilSeeds(eventType, addonInfo);
                        break;
                }
                break;
            case var a when a.StartsWith("Inventory"):
                var inv = AgentInventory.Instance();
                if (inv == null || inv->OpenTitleId != 15)
                    return;

                var ctx = inv->CurrentInventoryContextEvent;
                if (ctx == null)
                    return;

                if (C.HousingGardeningUseFertilizer && FindFertilizer() is { Slot: var slot, Type: var cont })
                {
                    Log("Fertilizing");
                    Service.TaskManager.EnqueueDelay(100); // this is needed cause PostShow is slightly too early. idk
                    Service.TaskManager.Enqueue(() => AgentInventory.Instance()->CurrentInventoryContextEvent->HandleCallback(slot, cont, 0, 0));
                }

                break;
        }
    }

    private void TrySelectSoilSeeds(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (_handled || eventType != AddonEvent.PreUpdate || !addonInfo.Addon.IsReady)
            return;

        var agent = AgentHousingPlant.Instance();
        if (agent == null || !agent->IsAgentActive())
            return;

        if (MemoryHelper.ReadField<uint>(agent, 0x38) != 0)
            return;

        ref var soilSel = ref agent->SelectedItems[0];
        ref var seedSel = ref agent->SelectedItems[1];
        if (soilSel.InventoryType != InventoryType.Invalid && seedSel.InventoryType != InventoryType.Invalid)
        {
            _handled = true;
            return;
        }

        var isOutdoors = agent->PlotType == 14;
        var soilPick = FindItem(C.HousingGardeningSoil, 21, isOutdoors, seed: false);
        var seedPick = FindItem(C.HousingGardeningSeed, 20, isOutdoors, seed: true);

        if (soilPick is { } soil && soilSel.InventoryType == InventoryType.Invalid)
            SelectItem(0, soil);
        if (seedPick is { } seed && seedSel.InventoryType == InventoryType.Invalid)
            SelectItem(1, seed);

        _handled = true;

        if (C.HousingGardeningConfirm && agent->SelectedItems[0].InventoryType != InventoryType.Invalid && agent->SelectedItems[1].InventoryType != InventoryType.Invalid)
        {
            Service.TaskManager.EnqueueDelay(100);
            Service.TaskManager.Enqueue(() =>
            {
                var a = AgentHousingPlant.Instance();
                if (a == null || !a->IsAgentActive())
                    return true;
                a->ConfirmSeedAndSoilSelection();
                return true;
            }, "HousingGardening.Confirm");
        }
    }

    private static (InventoryType Type, ushort Slot, uint ItemId, int Icon)? FindItem(uint preferredId, byte filterGroup, bool isOutdoors, bool seed)
    {
        var preferred = preferredId != 0 ? Locate(preferredId, filterGroup, isOutdoors, seed) : null;
        if (preferred != null)
            return preferred;

        if (!C.HousingGardeningFallback && preferredId != 0)
            return null;

        return Locate(0, filterGroup, isOutdoors, seed);
    }

    private static (InventoryType Type, ushort Slot, uint ItemId, int Icon)? FindFertilizer()
    {
        var preferredId = C.HousingGardeningFertilizer;
        var preferred = preferredId != 0 ? Locate(preferredId, 22, isOutdoors: false, seed: false) : null;
        if (preferred != null)
            return preferred;
        if (!C.HousingGardeningFallback && preferredId != 0)
            return null;
        return Locate(0, 22, isOutdoors: false, seed: false);
    }

    private static (InventoryType Type, ushort Slot, uint ItemId, int Icon)? Locate(uint requiredItemId, byte filterGroup, bool isOutdoors, bool seed)
    {
        var sheet = Svc.Data.GetExcelSheet<Item>();
        if (sheet == null)
            return null;

        var im = InventoryManager.Instance();
        for (var t = 0; t <= 3; t++)
        {
            var type = (InventoryType)t;
            var container = im->GetInventoryContainer(type);
            if (container == null || !container->IsLoaded)
                continue;

            for (ushort slot = 0; slot < container->Size; slot++)
            {
                var item = container->GetInventorySlot(slot);
                if (item == null || item->IsEmpty())
                    continue;

                var itemId = item->GetBaseItemId();
                if (requiredItemId != 0 && itemId != requiredItemId)
                    continue;

                if (!sheet.TryGetRow(itemId, out var row))
                    continue;
                if (row.ItemUICategory.RowId != 82 || row.FilterGroup != filterGroup)
                    continue;

                if (seed && isOutdoors && IsIndoorOnlySeed(row))
                    continue;

                return (type, slot, itemId, row.Icon);
            }
        }

        return null;
    }

    private static bool IsIndoorOnlySeed(Item item)
    {
        var seedSheet = Svc.Data.GetExcelSheet<GardeningSeed>();
        if (seedSheet == null || !seedSheet.TryGetRow(item.AdditionalData.RowId, out var seed))
            return false;
        return seed.IsPlantPotFlowerSeed;
    }

    private static void SelectItem(int index, (InventoryType Type, ushort Slot, uint ItemId, int Icon) pick)
    {
        var agent = AgentHousingPlant.Instance();
        if (agent == null || !agent->IsAgentActive())
            return;

        var numberArrayData = AtkStage.Instance()->GetNumberArrayData(NumberArrayType.Housing);
        if (numberArrayData == null || numberArrayData->IntArray == null || numberArrayData->Size < 2087)
            return;

        agent->SelectedItems[index].InventoryType = pick.Type;
        agent->SelectedItems[index].InventorySlot = pick.Slot;
        agent->SelectedItems[index].ItemId = pick.ItemId;

        numberArrayData->SetValue(3 * index + 2081, pick.Icon);
        numberArrayData->SetValue(3 * index + 2082, ((ushort)pick.Type << 16) | pick.Slot);

        InventoryManager.Instance()->SetSlotBlocked(pick.Type, (short)pick.Slot);
    }
}
