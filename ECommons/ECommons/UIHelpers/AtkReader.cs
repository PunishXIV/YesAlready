using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace ECommons.UIHelpers
{
    public unsafe abstract class AtkReader(AtkUnitBase* UnitBase, int BeginOffset = 0)
    {
        public List<T> Loop<T>(int Offset, int Size, int MaxLength) where T:AtkReader
        {
            var ret = new List<T>();
            for (int i = 0; i < MaxLength; i++)
            {
                var r = (AtkReader)Activator.CreateInstance(typeof(T), new object[] { (nint)UnitBase, Offset + i * Size });
                if (r.IsNull) break;
                ret.Add((T)r);
            }
            return ret;
        }

        public AtkReader(nint UnitBasePtr, int BeginOffset = 0) : this((AtkUnitBase*)UnitBasePtr, BeginOffset) { }

        public bool IsNull 
        {
            get 
            {
                if (UnitBase->AtkValuesCount == 0) return true;
                var num = 0 + BeginOffset;
                EnsureCount(UnitBase, num);
                if (UnitBase->AtkValues[num].Type == 0) return true;
                return false;
            }
        }
        protected uint? ReadUInt(int n)
        {
            var num = n + BeginOffset;
            EnsureCount(UnitBase, num);
            var value = UnitBase->AtkValues[num];
            if (value.Type == 0)
            {
                return null;
            }
            if (value.Type != ValueType.UInt) throw new InvalidCastException($"Value {num} from Addon {MemoryHelper.ReadStringNullTerminated((nint)UnitBase->Name)} was requested as uint but it was {value.Type}");
            return value.UInt;
        }

        protected int? ReadInt(int n)
        {
            var num = n + BeginOffset;
            EnsureCount(UnitBase, num);
            var value = UnitBase->AtkValues[num];
            if (value.Type == 0)
            {
                return null;
            }
            if (value.Type != ValueType.Int) throw new InvalidCastException($"Value {num} from Addon {MemoryHelper.ReadStringNullTerminated((nint)UnitBase->Name)} was requested as int but it was {value.Type}");
            return value.Int;
        }

        protected bool? ReadBool(int n)
        {
            var num = n + BeginOffset;
            EnsureCount(UnitBase, num);
            var value = UnitBase->AtkValues[num];
            if (value.Type == 0)
            {
                return null;
            }
            if (value.Type != ValueType.Bool) throw new InvalidCastException($"Value {num} from Addon {MemoryHelper.ReadStringNullTerminated((nint)UnitBase->Name)} was requested as bool but it was {value.Type}");
            return value.Byte != 0;
        }

        protected SeString? ReadSeString(int n)
        {
            var num = n + BeginOffset;
            EnsureCount(UnitBase, num);
            var value = UnitBase->AtkValues[num];
            if (value.Type == 0)
            {
                return null;
            }
            if (!value.Type.EqualsAny(ValueType.String, ValueType.AllocatedString)) throw new InvalidCastException($"Value {num} from Addon {MemoryHelper.ReadStringNullTerminated((nint)UnitBase->Name)} was requested as SeString but it was {value.Type}");
            return MemoryHelper.ReadSeStringNullTerminated((nint)value.String);
        }


        protected string? ReadString(int n)
        {
            var num = n + BeginOffset;
            EnsureCount(UnitBase, num);
            var value = UnitBase->AtkValues[num];
            if (value.Type == 0)
            {
                return null;
            }
            if (!value.Type.EqualsAny(ValueType.String, ValueType.AllocatedString)) throw new InvalidCastException($"Value {num} from Addon {MemoryHelper.ReadStringNullTerminated((nint)UnitBase->Name)} was requested as String but it was {value.Type}");
            return MemoryHelper.ReadStringNullTerminated((nint)value.String);
        }

        void EnsureCount(AtkUnitBase* Addon, int num)
        {
            if (num >= Addon->AtkValuesCount) throw new ArgumentOutOfRangeException(nameof(num));
        }
    }
}
