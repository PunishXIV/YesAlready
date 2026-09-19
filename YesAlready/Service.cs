using ECommons.Automation.NeoTaskManager;
using Lumina.Excel.Sheets;
using System.Linq;
using YesAlready.IPC;
using YesAlready.UI;

namespace YesAlready;

public static class Service
{
    public static TaskManager TaskManager { get; private set; } = null!;
    public static BlockListHandler BlockListHandler { get; private set; } = null!;
    public static YesAlreadyIPC IPC { get; private set; } = null!;
    public static Watcher Watcher { get; private set; } = null!;
    public static BotherBadges BotherBadges { get; private set; } = null!;

    public static string[] QuestNames = [.. Svc.Data.GetExcelSheet<Quest>()!.Where(q => !q.Name.IsEmpty).Select(q => q.Name.GetText())];
}
