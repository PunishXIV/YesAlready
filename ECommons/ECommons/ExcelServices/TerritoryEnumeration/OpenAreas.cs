using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ECommons.ExcelServices.TerritoryEnumeration
{
    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    public static class OpenAreas // 1
    {
        public const ushort Middle_La_Noscea = 134;
        public const ushort Lower_La_Noscea = 135;
        public const ushort Eastern_La_Noscea = 137;
        public const ushort Western_La_Noscea = 138;
        public const ushort Upper_La_Noscea = 139;
        public const ushort Western_Thanalan = 140;
        public const ushort Central_Thanalan = 141;
        public const ushort Eastern_Thanalan = 145;
        public const ushort Southern_Thanalan = 146;
        public const ushort Northern_Thanalan = 147;
        public const ushort Central_Shroud = 148;
        public const ushort East_Shroud = 152;
        public const ushort South_Shroud = 153;
        public const ushort North_Shroud = 154;
        public const ushort Coerthas_Central_Highlands = 155;
        public const ushort Mor_Dhona = 156;
        public const ushort Outer_La_Noscea = 180;
        public const ushort Wolves_Den_Pier = 250;
        public const ushort Coerthas_Western_Highlands = 397;
        public const ushort The_Dravanian_Forelands = 398;
        public const ushort The_Dravanian_Hinterlands = 399;
        public const ushort The_Churning_Mists = 400;
        public const ushort The_Sea_of_Clouds = 401;
        public const ushort Azys_Lla = 402;
        public const ushort The_Fringes = 612;
        public const ushort The_Ruby_Sea = 613;
        public const ushort Yanxia = 614;
        public const ushort The_Peaks = 620;
        public const ushort The_Lochs = 621;
        public const ushort The_Azim_Steppe = 622;
        public const ushort The_Diadem = 630;
        public const ushort Lakeland = 813;
        public const ushort Kholusia = 814;
        public const ushort Amh_Araeng = 815;
        public const ushort Il_Mheg = 816;
        public const ushort The_Raktika_Greatwood = 817;
        public const ushort The_Tempest = 818;
        public const ushort Labyrinthos = 956;
        public const ushort Thavnair = 957;
        public const ushort Garlemald = 958;
        public const ushort Mare_Lamentorum = 959;
        public const ushort Ultima_Thule = 960;
        public const ushort Elpis = 961;
        public const ushort Castrum_Marinum_Drydocks = 967;

        static ushort[] list = null;
        public static ushort[] List
        {
            get
            {
                if (list == null)
                {
                    var s = new List<ushort>();
                    typeof(OpenAreas).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Each(x => s.Add((ushort)x.GetValue(null)));
                    list = s.ToArray();
                }
                return list;
            }
        }
    }
}
