using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommons.UIHelpers.Implementations
{
    public unsafe class ReaderSelectString(AtkUnitBase* a) : AtkReader(a)
    {
        public string Description => ReadString(2);
        public int NumEntries => ReadInt(3) ?? 0;
        public List<Entry> Entries => Loop<Entry>(7, 1, NumEntries); 

        public unsafe class Entry(nint a, int s) : AtkReader(a, s)
        {
            public string Text => ReadString(0);
        }
    }
}
