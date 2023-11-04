using ECommons.Logging;
using ECommons.DalamudServices;
using System;

namespace ECommons.Schedulers;

public class ExecuteForScheduler : IScheduler
{
    long stopExecAt;
    Action function;
    bool disposed = false;

    public ExecuteForScheduler(Action function, long executeForMS)
    {
        stopExecAt = Environment.TickCount64 + executeForMS;
        this.function = function;
        Svc.Framework.Update += Execute;
    }

    public void Dispose()
    {
        if (!disposed)
        {
            Svc.Framework.Update -= Execute;
        }
        disposed = true;
    }

    void Execute(object _)
    {
        try
        {
            function();
        }
        catch (Exception e)
        {
            PluginLog.Error(e.Message + "\n" + e.StackTrace ?? "");
        }
        if (Environment.TickCount64 > stopExecAt)
        {
            Dispose();
        }
    }
}