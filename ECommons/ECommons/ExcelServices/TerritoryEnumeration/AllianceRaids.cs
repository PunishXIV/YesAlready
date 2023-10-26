using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ECommons.ExcelServices.TerritoryEnumeration
{
    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    public static class AllianceRaids
    {
        public const ushort the_World_of_Darkness = 151;
        public const ushort the_Labyrinth_of_the_Ancients = 174;
        public const ushort Syrcus_Tower = 372;
        public const ushort the_Void_Ark = 508;
        public const ushort the_Weeping_City_of_Mhach = 556;
        public const ushort Dun_Scaith = 627;
        public const ushort the_Royal_City_of_Rabanastre = 734;
        public const ushort the_Ridorana_Lighthouse = 776;
        public const ushort the_Orbonne_Monastery = 826;
        public const ushort The_Copied_Factory = 882;
        public const ushort the_Puppets_Bunker = 917;
        public const ushort The_Tower_at_Paradigms_Breach = 966;
        public const ushort Aglaia = 1054;

        static ushort[] list = null;
        public static ushort[] List
        {
            get
            {
                if (list == null)
                {
                    var s = new List<ushort>();
                    typeof(AllianceRaids).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Each(x => s.Add((ushort)x.GetValue(null)));
                    list = s.ToArray();
                }
                return list;
            }
        }
    }
}
