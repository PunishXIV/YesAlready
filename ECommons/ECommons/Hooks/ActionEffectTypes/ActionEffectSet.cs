using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.GeneratedSheets;
using System.Collections.Generic;
using System.Numerics;
using Action = Lumina.Excel.GeneratedSheets.Action;
using Character = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

namespace ECommons.Hooks.ActionEffectTypes;

public unsafe struct ActionEffectSet
{
    public Action Action { get; }

    public Item Item { get; }

    public EventItem EventItem { get; }

    public Mount Mount { get; }

    public ushort IconId { get; }

    public string Name { get; }

    public GameObject Target { get; }

    public GameObject Source { get; }

    public Character SourceCharacter { get; }

    public TargetEffect[] TargetEffects { get; }

    public Vector3 Position { get; }

    public EffectHeader Header { get; }

    public ActionEffectSet(uint sourceID, Character* sourceCharacter, Vector3* pos, EffectHeader* effectHeader, EffectEntry* effectArray, ulong* effectTail)
    {
        switch (effectHeader->ActionType)
        {
            case ActionType.KeyItem:
                EventItem = Svc.Data.GetExcelSheet<EventItem>().GetRow(effectHeader->ActionID);
                Name = EventItem?.Singular ?? string.Empty;
                IconId = EventItem?.Icon ?? 0;
                break;

            case ActionType.Item:
                var id = effectHeader->ActionID > 1000000 ? effectHeader->ActionID - 1000000 : effectHeader->ActionID;
                Item = Svc.Data.GetExcelSheet<Item>().GetRow(id);
                Name = Item?.Name ?? string.Empty;
                IconId = Item?.Icon ?? 0;
                break;

            case ActionType.Mount:
                Mount = Svc.Data.GetExcelSheet<Mount>().GetRow(effectHeader->ActionID);
                Name = Mount?.Singular ?? string.Empty;
                IconId = Mount?.Icon ?? 0;
                break;

            default:
                Action = Svc.Data.GetExcelSheet<Action>().GetRow(effectHeader->ActionID);
                Name = Action?.Name ?? string.Empty; ;

                var actionCate = Action?.ActionCategory.Value?.RowId ?? 0;

                IconId = actionCate == 1 ? (ushort)101 // Auto Attack
                    : effectHeader->ActionID == 3 ? (ushort)104 //Sprint
                    : effectHeader->ActionID == 4 ? (ushort)118 //Mount
                    : Action?.Icon ?? 0;
                break;
        }
        Target = Svc.Objects.SearchById(effectHeader->AnimationTargetId);
        Source = Svc.Objects.SearchById(sourceID);
        SourceCharacter = *sourceCharacter;
        Position = *pos;
        Header = *effectHeader;

        TargetEffects = new TargetEffect[effectHeader->TargetCount];
        for (int i = 0; i < effectHeader->TargetCount; i++)
        {
            TargetEffects[i] = new TargetEffect(effectTail[i], effectArray + 8 * i);
        }
    }

    public Dictionary<ulong, uint> GetSpecificTypeEffect(ActionEffectType type)
    {
        var result = new Dictionary<ulong, uint>();
        foreach (var effect in TargetEffects)
        {
            if (effect.GetSpecificTypeEffect(type, out var e))
            {
                //Is this value or Damage? IDK about it.
                result[effect.TargetID] = e.value;
            }
        }
        return result;
    }

    public override string ToString()
    {
        var str = $"S:{Source?.Name}, T:{Target?.Name}, Lock:{Header.AnimationLockTime}";
        str += $"\nType: {Header.ActionType}, Name: {Action?.Name}({Action?.RowId})";
        if (TargetEffects != null)
        {
            foreach (var effect in TargetEffects)
            {
                str += "\n" + effect.ToString();
            }
        }
        return str;
    }
}
