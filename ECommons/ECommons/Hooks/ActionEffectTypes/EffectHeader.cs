using FFXIVClientStructs.FFXIV.Client.Game;
using System.Runtime.InteropServices;

namespace ECommons.Hooks.ActionEffectTypes
{
    [StructLayout(LayoutKind.Explicit)]
    public struct EffectHeader
    {
        [FieldOffset(0)] public ulong AnimationTargetId;
        [FieldOffset(8)] public uint ActionID;
        [FieldOffset(12)] public uint GlobalEffectCounter;
        [FieldOffset(16)] public float AnimationLockTime;
        [FieldOffset(20)] public uint SomeTargetID;
        [FieldOffset(24)] public ushort SourceSequence;
        [FieldOffset(26)] public ushort Rotation;
        [FieldOffset(28)] public ushort AnimationId;
        [FieldOffset(30)] public byte Variation;
        [FieldOffset(31)] public ActionType ActionType;
        [FieldOffset(33)] public byte TargetCount;
    }
}
