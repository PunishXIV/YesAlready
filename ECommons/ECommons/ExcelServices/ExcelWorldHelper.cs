using ECommons.DalamudServices;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Linq;

namespace ECommons.ExcelServices;

public static class ExcelWorldHelper
{
    public static World GetWorldByName(string name)
    {
        if(Svc.Data.GetExcelSheet<World>().TryGetFirst(x => x.Name.ToString().EqualsIgnoreCase(name), out var result))
        {
            return result;
        }
        return null;
    }

    public static bool? TryGetWorldByName(string name, out World result)
    {
        result = GetWorldByName(name);
        return result != null;
    }

    public static World[] GetPublicWorlds(Region? region)
    {
        return Svc.Data.GetExcelSheet<World>().Where(x => ((region == null && x.Region.EqualsAny(Enum.GetValues<Region>().Select(z => (byte)z).ToArray())) || (region.HasValue && x.Region == (byte)region.Value)) && x.IsPublic).ToArray();
    }

    public static World GetWorldById(uint id)
    {
        return Svc.Data.GetExcelSheet<World>().GetRow(id);
    }

    public static string GetWorldNameById(uint id)
    {
        return GetWorldById(id)?.Name.ToString();
    }

    public static World GetPublicWorldById(uint id)
    {
        var data = Svc.Data.GetExcelSheet<World>().GetRow(id);
        if (data.Region.EqualsAny(Enum.GetValues<Region>().Select(z => (byte)z).ToArray()))
        {
            return data;
        }
        return null;
    }


    public static string GetPublicWorldNameById(uint id)
    {
        return GetPublicWorldById(id)?.Name.ToString();
    }
}
