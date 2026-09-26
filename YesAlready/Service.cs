using ECommons.Automation.NeoTaskManager;
using Lumina.Excel.Sheets;
using System.Collections.Generic;
using System.Linq;
using YesAlready.IPC;

namespace YesAlready;

public static class Service
{
    public static TaskManager TaskManager { get; private set; } = null!;
    public static BlockListHandler BlockListHandler { get; private set; } = null!;
    public static YesAlreadyIPC IPC { get; private set; } = null!;
    public static Watcher Watcher { get; private set; } = null!;

    public static Dictionary<uint, string> Quests = Quest.Where(q => !q.Name.IsEmpty).ToDictionary(k => k.RowId, v => v.Name.GetText());
}
