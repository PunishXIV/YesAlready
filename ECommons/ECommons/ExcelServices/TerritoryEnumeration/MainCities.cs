using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ECommons.ExcelServices.TerritoryEnumeration
{
    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    //intended use 0
    public static class MainCities 
    {
        public const ushort Limsa_Lominsa_Upper_Decks = 128;
        public const ushort Limsa_Lominsa_Lower_Decks = 129;
        public const ushort Uldah_Steps_of_Nald = 130;
        public const ushort Uldah_Steps_of_Thal = 131;
        public const ushort New_Gridania = 132;
        public const ushort Old_Gridania = 133;
        public const ushort Mist = 136;
        public const ushort Foundation = 418;
        public const ushort The_Pillars = 419;
        public const ushort Idyllshire = 478;
        public const ushort Kugane = 628;
        public const ushort Rhalgrs_Reach = 635;
        public const ushort The_Doman_Enclave = 759;
        public const ushort The_Crystarium = 819;
        public const ushort Eulmore = 820;
        public const ushort Old_Sharlayan = 962;
        public const ushort Radz_at_Han = 963;

        static ushort[] list = null;
        public static ushort[] List
        {
            get
            {
                if (list == null)
                {
                    var s = new List<ushort>();
                    typeof(MainCities).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Each(x => s.Add((ushort)x.GetValue(null)));
                    list = s.ToArray();
                }
                return list;
            }
        }
    }
}
