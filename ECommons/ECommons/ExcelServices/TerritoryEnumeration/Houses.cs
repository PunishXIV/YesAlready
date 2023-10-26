using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ECommons.ExcelServices.TerritoryEnumeration
{
    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    public static class Houses
    {
        public const ushort Private_Cottage_Mist = 282;
        public const ushort Private_House_Mist = 283;
        public const ushort Private_Mansion_Mist = 284;
        public const ushort Private_Cottage_The_Lavender_Beds = 342;
        public const ushort Private_House_The_Lavender_Beds = 343;
        public const ushort Private_Mansion_The_Lavender_Beds = 344;
        public const ushort Private_Cottage_The_Goblet = 345;
        public const ushort Private_House_The_Goblet = 346;
        public const ushort Private_Mansion_The_Goblet = 347;
        public const ushort Private_Chambers_Mist = 384;
        public const ushort Private_Chambers_The_Lavender_Beds = 385;
        public const ushort Private_Chambers_The_Goblet = 386;
        public const ushort Company_Workshop_Mist = 423;
        public const ushort Company_Workshop_The_Goblet = 424;
        public const ushort Company_Workshop_The_Lavender_Beds = 425;
        public const ushort Topmast_Apartment_Lobby = 573;
        public const ushort Lily_Hills_Apartment_Lobby = 574;
        public const ushort Sultanas_Breath_Apartment_Lobby = 575;
        public const ushort Topmast_Apartment = 608;
        public const ushort Lily_Hills_Apartment = 609;
        public const ushort Sultanas_Breath_Apartment = 610;
        public const ushort Private_Cottage_Shirogane = 649;
        public const ushort Private_House_Shirogane = 650;
        public const ushort Private_Mansion_Shirogane = 651;
        public const ushort Private_Chambers_Shirogane = 652;
        public const ushort Company_Workshop_Shirogane = 653;
        public const ushort Kobai_Goten_Apartment_Lobby = 654;
        public const ushort Kobai_Goten_Apartment = 655;
        public const ushort Private_Cottage_Empyreum = 980;
        public const ushort Private_House_Empyreum = 981;
        public const ushort Private_Mansion_Empyreum = 982;
        public const ushort Private_Chambers_Empyreum = 983;
        public const ushort Company_Workshop_Empyreum = 984;
        public const ushort Ingleside_Apartment_Lobby = 985;
        public const ushort Ingleside_Apartment = 999;

        static ushort[] list = null;
        public static ushort[] List
        {
            get
            {
                if (list == null)
                {
                    var s = new List<ushort>();
                    typeof(Houses).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Each(x => s.Add((ushort)x.GetValue(null)));
                    list = s.ToArray();
                }
                return list;
            }
        }
    }
}
