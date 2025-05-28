using ECommons.Automation.NeoTaskManager;
using YesAlready.IPC;

namespace YesAlready;
public static class Service
{
    public static TaskManager TaskManager { get; private set; } = null!;
    public static BlockListHandler BlockListHandler { get; private set; } = null!;
    public static YesAlreadyIPC IPC { get; private set; } = null!;
    public static Watcher Watcher { get; private set; } = null!;
}
