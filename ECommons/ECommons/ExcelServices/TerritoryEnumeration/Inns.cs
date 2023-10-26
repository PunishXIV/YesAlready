using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ECommons.ExcelServices.TerritoryEnumeration
{
    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    public static class Inns
    {
        public const ushort Mizzenmast_Inn = 177;
        public const ushort The_Hourglass = 178;
        public const ushort The_Roost = 179;
        public const ushort Cloud_Nine = 429;
        public const ushort Bokairo_Inn = 629;
        public const ushort The_Pendants_Personal_Suite = 843;
        public const ushort Andron = 990;

        static ushort[] list = null;
        public static ushort[] List
        {
            get
            {
                if (list == null)
                {
                    var s = new List<ushort>();
                    typeof(Inns).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Each(x => s.Add((ushort)x.GetValue(null)));
                    list = s.ToArray();
                }
                return list;
            }
        }
    }
}
