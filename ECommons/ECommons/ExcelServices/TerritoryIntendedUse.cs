using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ECommons.ExcelServices
{
    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    public enum TerritoryIntendedUse : uint
    {
        Unknown = 0,
        AllianceRaid,
        Dungeon,
        House,
        Inn,
        MainCity,
        OpenArea,
        Prison,
        Raid,
        OldRaid,
        Residential,
        Trial,
        Variant
    }
}
