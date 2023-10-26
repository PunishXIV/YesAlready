using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommons.Automation
{
    internal record class TaskManagerTask
    {
        internal Func<bool?> Action;
        internal int TimeLimitMS;
        internal bool AbortOnTimeout;
        internal string Name;

        internal TaskManagerTask(Func<bool?> action, int timeLimitMS, bool abortOnTimeout, string name)
        {
            Action = action;
            TimeLimitMS = timeLimitMS;
            AbortOnTimeout = abortOnTimeout;
            Name = name;// ?? new StackTrace().GetFrames().Select(x => x.GetMethod()?.Name ?? "<unknown>").Join(" <- ");
        }
    }
}
