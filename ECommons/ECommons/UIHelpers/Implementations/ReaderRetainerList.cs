using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommons.UIHelpers.Implementations
{
    public unsafe class ReaderRetainerList(AtkUnitBase* Addon) : AtkReader(Addon)
    {
        public uint VentureCount => this.ReadUInt(2) ?? 0;
        public List<Retainer> Retainers => this.Loop<Retainer>(3, 9, 10);

        public unsafe class Retainer(nint Addon, int start) : AtkReader(Addon, start)
        {
            public string Name => ReadString(0);
            public uint Level => ReadUInt(2) ?? 0;
            public uint Inventory => ReadUInt(3) ?? 0;
            public uint Gil => ReadUInt(4) ?? 0;
            public bool IsActive => ReadBool(8) ?? false;
        }
    }
}
