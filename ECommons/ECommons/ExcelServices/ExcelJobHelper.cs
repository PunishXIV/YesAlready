using ECommons.DalamudServices;
using Lumina.Excel.GeneratedSheets;
using System.Linq;

namespace ECommons.ExcelServices;

public static class ExcelJobHelper
{
    public static ClassJob GetJobByName(string name)
    {
        if (Svc.Data.GetExcelSheet<ClassJob>().TryGetFirst(x => x.Name.ToString().EqualsIgnoreCase(name), out var result))
        {
            return result;
        }
        return null;
    }

    public static bool TryGetJobByName(string name, out ClassJob result)
    {
        result = GetJobByName(name);
        return result != null;
    }

    public static ClassJob GetJobById(uint id)
    {
        return Svc.Data.GetExcelSheet<ClassJob>().GetRow(id);
    }

    public static string GetJobNameById(uint id)
    {
        return GetJobById(id)?.Name.ToString();
    }

    public static ClassJob[] GetCombatJobs()
    {
        return Svc.Data.GetExcelSheet<ClassJob>().Where(x => x.Role.EqualsAny<byte>(2, 3)).ToArray();
    }
}
