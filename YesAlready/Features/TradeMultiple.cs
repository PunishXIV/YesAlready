using Dalamud.Game.Inventory;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using System;
using System.Collections.Generic;
using System.Linq;

namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
[AddonFeature(AddonEvent.PostShow)]
[AddonFeature(AddonEvent.PostHide)]
[Bother(nameof(Configuration.TradeMultiple), BotherCategory.Materia, "Auto-fill and confirm Materia Transmutation.")]
internal class TradeMultiple : AddonFeature
{
    private static bool _busy;
    private static bool _awaitingReshow; // after confim+hide

    private readonly record struct Pick(InventoryType Container, short Slot, MateriaItemInfo Info, int Quantity);
    private readonly record struct AvailableMateria(InventoryType Container, short Slot, uint ItemId, int Quantity, MateriaItemInfo Info);

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (eventType == AddonEvent.PostHide)
        {
            if (!_awaitingReshow) // keep _busy while waiting for the post-transmute re-show so we don't re-enter mid-animation.
                _busy = false;
            return;
        }

        if (_awaitingReshow)
        {
            _awaitingReshow = false;
            _busy = false;
            Log($"{eventType}: window re-appear, starting next cycle");
        }

        if (_busy)
            return;

        if (IGameGui.Get().TryGetAddon<AtkUnitBase>("SelectYesno", out _) || IGameGui.Get().TryGetAddon<AtkUnitBase>("InputNumeric", out _))
            return;

        Log($"{eventType}: scheduling fill");
        _busy = true;
        EnqueueWhenReady();
    }

    private unsafe void EnqueueWhenReady()
    {
        var deadline = Environment.TickCount64 + 5000;
        Service.TaskManager.Enqueue(() =>
        {
            if (Environment.TickCount64 > deadline)
            {
                Log("Timed out waiting for TradeMultiple to become ready");
                _busy = false;
                return true;
            }

            if (!GenericHelpers.TryGetAddonByName<AddonTradeMultiple>("TradeMultiple", out var addon) || !addon->AtkUnitBase.IsAddonReady())
                return false;

            var agent = AgentTradeMultiple.Instance();
            if (agent == null || agent->MaxQuantity == 0)
                return false;

            if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("SelectYesno", out _) || GenericHelpers.TryGetAddonByName<AtkUnitBase>("InputNumeric", out _))
                return false;

            var remaining = agent->GetSlotsRemaining();
            Log($"Ready: remaining={remaining}/{agent->MaxQuantity}, showErrors={agent->ShowErrorMessages}, handler={(nint)agent->Handler:X}");

            if (remaining == 0)
            {
                Submit(agent);
                return true;
            }

            StartFill(agent);
            return true;
        }, "TradeMultiple.WaitReady");
    }

    private unsafe void StartFill(AgentTradeMultiple* agent)
    {
        var needed = (int)agent->GetSlotsRemaining();
        var picks = SelectPicks(agent);
        var totalQty = picks.Sum(p => p.Quantity);
        if (picks.Count == 0 || totalQty < needed)
        {
            Log($"Not enough materia to fill ({totalQty}/{needed}, mode={C.TransmuteMode})");
            _busy = false;
            return;
        }

        Log($"Filling {totalQty}/{needed} piece(s) via {C.TransmuteMode}");

        foreach (var pick in picks)
        {
            var p = pick;
            uint remainingBefore = 0;
            var waitDeadline = 0L;
            Service.TaskManager.Enqueue(() =>
            {
                var a = AgentTradeMultiple.Instance();
                if (a == null || a->MaxQuantity == 0)
                    return true;

                remainingBefore = a->GetSlotsRemaining();
                if (remainingBefore == 0)
                    return true;

                var item = InventoryManager.Instance()->GetInventorySlot(p.Container, p.Slot);
                if (item == null || item->IsEmpty() || item->GetBaseItemId() != p.Info.ItemId)
                {
                    Log($"Skipping missing materia [#{p.Info.ItemId}] {p.Info.ItemName} at {p.Container}:{p.Slot}");
                    return true;
                }

                var qty = (uint)Math.Min(p.Quantity, (int)remainingBefore);
                Log($"Adding {qty}x [#{p.Info.ItemId}] {p.Info.ItemName} from {p.Container}:{p.Slot} (remaining={remainingBefore})");
                a->AddItemQuantity(item, qty);
                waitDeadline = Environment.TickCount64 + 3000;
                return true;
            }, $"TradeMultiple.Add:{p.Info.ItemId}");

            Service.TaskManager.Enqueue(() =>
            {
                if (waitDeadline != 0 && Environment.TickCount64 > waitDeadline)
                {
                    Log($"Timed out waiting for add of [#{p.Info.ItemId}] {p.Info.ItemName}");
                    return true;
                }

                var a = AgentTradeMultiple.Instance();
                if (a == null || a->MaxQuantity == 0)
                    return true;

                return a->GetSlotsRemaining() < remainingBefore;
            }, $"TradeMultiple.Wait:{p.Info.ItemId}");
        }

        Service.TaskManager.Enqueue(() =>
        {
            var a = AgentTradeMultiple.Instance();
            if (a == null)
            {
                _busy = false;
                return true;
            }

            if (a->GetSlotsRemaining() == 0)
                Submit(a);
            else
                _busy = false;

            return true;
        }, "TradeMultiple.AfterFill");
    }

    private unsafe void Submit(AgentTradeMultiple* agent)
    {
        LogSlots(agent);

        if (HasDuplicateInventorySlots(agent))
        {
            Log("Submit aborted, duplicated inv slots");
            _busy = false;
            return;
        }

        if (agent->Handler == null)
        {
            Log("Submit aborted, handler null");
            _busy = false;
            return;
        }

        if (!agent->Confirm())
        {
            Log("Confirm returned false");
            _busy = false;
            return;
        }

        Log($"Hiding, awaiting reshow");
        _awaitingReshow = true;
        agent->Hide();
    }

    public override void Disable()
    {
        base.Disable();
        Service.TaskManager.Abort();
        _busy = false;
        _awaitingReshow = false;
    }

    private unsafe void LogSlots(AgentTradeMultiple* agent)
    {
        var i = 0;
        foreach (ref var slot in agent->Slots)
        {
            if (slot.Container != InventoryType.Invalid)
                Log($"Slot[{i}]: {slot.Container}:{slot.Slot} item={slot.ItemId} qty={slot.Quantity}");
            i++;
        }
    }

    private static unsafe bool HasDuplicateInventorySlots(AgentTradeMultiple* agent)
    {
        var seen = new HashSet<(InventoryType, short)>();
        foreach (ref var slot in agent->Slots)
        {
            if (slot.Container == InventoryType.Invalid || slot.Quantity <= 0)
                continue;
            if (!seen.Add((slot.Container, slot.Slot)))
                return true;
        }

        return false;
    }

    private unsafe List<Pick> SelectPicks(AgentTradeMultiple* agent)
    {
        var available = ScanInventory(agent);
        if (available.Count == 0)
            return [];

        var (usedTypes, usedGrade) = GetUsedMateria(agent);
        var remaining = (int)agent->GetSlotsRemaining();
        if (remaining <= 0)
            return [];

        return C.TransmuteMode == Configuration.TradeMultipleMode.AllSame ? SelectAllSame(available, usedTypes, usedGrade, remaining) : SelectAllDifferent(available, usedTypes, usedGrade, remaining);
    }

    private static unsafe (HashSet<uint> Types, byte? Grade) GetUsedMateria(AgentTradeMultiple* agent)
    {
        var used = new HashSet<uint>();
        var gradeCounts = new Dictionary<byte, int>();
        var catalog = MateriaCatalog.Get();
        foreach (ref var slot in agent->Slots)
        {
            if (slot.Container == InventoryType.Invalid || slot.ItemId == 0 || slot.Quantity <= 0)
                continue;
            if (!catalog.TryGet(slot.ItemId, out var info))
                continue;

            used.Add(info.MateriaRowId);
            gradeCounts[info.Grade] = gradeCounts.GetValueOrDefault(info.Grade) + slot.Quantity;
        }

        // lowest grade present, keep going bottom up
        // logic being that you have a chance to get a higher materia so start low and work your way up, otherwise if you did randomly you might end up with weird mixes by the end
        byte? grade = gradeCounts.Count == 0 ? null : gradeCounts.Keys.Min();
        return (used, grade);
    }

    private static List<Pick> SelectAllSame(List<AvailableMateria> available, HashSet<uint> usedTypes, byte? usedGrade, int remaining)
    {
        uint? forcedType = usedTypes.Count == 1 ? usedTypes.First() : null;

        var groups = available
            .Where(a => forcedType is null || a.Info.MateriaRowId == forcedType)
            .Where(a => usedGrade is null || a.Info.Grade >= usedGrade)
            .GroupBy(a => (a.Info.MateriaRowId, a.Info.Grade))
            .Select(g => new
            {
                Count = g.Sum(x => x.Quantity),
                g.Key.Grade,
                Stacks = g.OrderByDescending(x => x.Quantity).ToList(),
            })
            .ToList();

        // prefer a single type+grade that can fill the window, lowest grade first
        var best = groups
            .Where(g => g.Count >= remaining)
            .OrderBy(g => g.Grade)
            .ThenByDescending(g => g.Count)
            .FirstOrDefault();

        if (best is not null)
            return TakeFromStacks(best.Stacks, remaining);

        // nothing with enough of one materia, mix bottom-up by grade
        var picks = new List<Pick>();
        var need = remaining;
        foreach (var group in groups.OrderBy(g => g.Grade).ThenByDescending(g => g.Count))
        {
            if (need <= 0)
                break;
            foreach (var stack in group.Stacks)
            {
                if (need <= 0)
                    break;
                var take = Math.Min(need, stack.Quantity);
                picks.Add(new Pick(stack.Container, stack.Slot, stack.Info, take));
                need -= take;
            }
        }

        return picks;
    }

    private static List<Pick> TakeFromStacks(List<AvailableMateria> stacks, int need)
    {
        var picks = new List<Pick>();
        foreach (var stack in stacks)
        {
            if (need <= 0)
                break;
            var take = Math.Min(need, stack.Quantity);
            picks.Add(new Pick(stack.Container, stack.Slot, stack.Info, take));
            need -= take;
        }

        return picks;
    }

    private List<Pick> SelectAllDifferent(List<AvailableMateria> available, HashSet<uint> usedTypes, byte? usedGrade, int remaining)
    {
        // remaining qty per stack is mutable so it can be bumped instead of submitting two 1x refs to the same slot (server rejects it)
        var left = available.ToDictionary(a => (a.Container, a.Slot), a => a.Quantity);
        var bySlot = available.ToDictionary(a => (a.Container, a.Slot));

        var picks = new List<Pick>();
        var pickIndex = new Dictionary<(InventoryType, short), int>();
        var selectedTypes = new HashSet<uint>(usedTypes);

        void Take(AvailableMateria stack, int qty)
        {
            var key = (stack.Container, stack.Slot);
            left[key] -= qty;
            if (pickIndex.TryGetValue(key, out var idx))
                picks[idx] = picks[idx] with { Quantity = picks[idx].Quantity + qty };
            else
            {
                pickIndex[key] = picks.Count;
                picks.Add(new Pick(stack.Container, stack.Slot, stack.Info, qty));
            }
        }

        int Need() => remaining - picks.Sum(p => p.Quantity);

        var minGrade = usedGrade ?? available.Min(a => (byte?)a.Info.Grade);
        if (minGrade is null)
            return [];

        var grades = available
            .Select(a => a.Info.Grade)
            .Where(g => g >= minGrade)
            .Distinct()
            .OrderBy(g => g)
            .ToList();

        // only lock to one grade when it has enough unique types; otherwise spread across grades bottom-up
        byte? soloGrade = null;
        foreach (var grade in grades)
        {
            var unique = available
                .Where(a => a.Info.Grade == grade && left[(a.Container, a.Slot)] > 0)
                .Select(a => a.Info.MateriaRowId)
                .Distinct()
                .Count(id => !selectedTypes.Contains(id));
            if (unique >= remaining)
            {
                soloGrade = grade;
                break;
            }
        }

        var gradeOrder = soloGrade is { } sg ? [sg] : grades;
        foreach (var grade in gradeOrder)
        {
            while (Need() > 0)
            {
                var candidate = available
                    .Where(a => a.Info.Grade == grade && left[(a.Container, a.Slot)] > 0)
                    .Where(a => !selectedTypes.Contains(a.Info.MateriaRowId))
                    .GroupBy(a => a.Info.MateriaRowId)
                    .Select(g =>
                    {
                        var stack = g.OrderByDescending(x => left[(x.Container, x.Slot)]).First();
                        return new { Stack = stack, TypeCount = g.Sum(x => left[(x.Container, x.Slot)]) };
                    })
                    .OrderByDescending(x => x.TypeCount)
                    .FirstOrDefault();

                if (candidate is null)
                    break;

                Take(candidate.Stack, 1);
                selectedTypes.Add(candidate.Stack.Info.MateriaRowId);
            }

            if (Need() <= 0)
                break;
        }

        if (Need() <= 0)
            return picks;

        if (C.TradeMultipleRequireUnique)
        {
            Log($"Not enough unique materia, stopping ({picks.Sum(p => p.Quantity)}/{remaining})");
            return [];
        }

        // pad with dupes from lowest grade / largest stacks
        // goal is to keep the spread of grades tight
        while (Need() > 0)
        {
            var key = left
                .Where(kv => kv.Value > 0 && (soloGrade is null || bySlot[kv.Key].Info.Grade == soloGrade))
                .OrderBy(kv => bySlot[kv.Key].Info.Grade)
                .ThenByDescending(kv => pickIndex.ContainsKey(kv.Key))
                .ThenByDescending(kv => kv.Value)
                .Select(kv => ((InventoryType, short)?)kv.Key)
                .FirstOrDefault();

            if (key is not { } slotKey)
                break;

            Take(bySlot[slotKey], Math.Min(Need(), left[slotKey]));
        }

        return picks;
    }

    private unsafe List<AvailableMateria> ScanInventory(AgentTradeMultiple* agent)
    {
        var blocked = new HashSet<(InventoryType, short)>();
        foreach (ref var slot in agent->Slots)
        {
            if (slot.Container != InventoryType.Invalid)
                blocked.Add((slot.Container, slot.Slot));
        }

        var list = new List<AvailableMateria>();
        InventoryType[] bags =
        [
            InventoryType.Inventory1,
            InventoryType.Inventory2,
            InventoryType.Inventory3,
            InventoryType.Inventory4,
        ];

        var catalog = MateriaCatalog.Get();
        foreach (var bag in bags)
        {
            foreach (var item in IGameInventory.Get().GetInventoryItems((GameInventoryType)bag))
            {
                if (item.IsEmpty || item.Quantity <= 0)
                    continue;
                if (catalog.IsExcludedFromTransmute(item.BaseItemId) || !catalog.TryGet(item.BaseItemId, out var info))
                    continue;
                if (blocked.Contains(((InventoryType)item.ContainerType, (short)item.InventorySlot)))
                    continue;

                list.Add(new AvailableMateria((InventoryType)item.ContainerType, (short)item.InventorySlot, item.BaseItemId, item.Quantity, info));
            }
        }

        return list;
    }
}
