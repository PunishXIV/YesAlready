using Dalamud.Interface.Colors;
using ECommons.ImGuiMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommons.Throttlers
{
    public class EzThrottler<T>
    {
        Dictionary<T, long> throttlers = new();
        public IReadOnlyCollection<T> ThrottleNames => throttlers.Keys;
        public bool Throttle(T name, int miliseconds = 500, bool rethrottle = false)
        {
            if (!throttlers.ContainsKey(name))
            {
                throttlers[name] = Environment.TickCount64 + miliseconds;
                return true;
            }
            if (Environment.TickCount64 > throttlers[name])
            {
                throttlers[name] = Environment.TickCount64 + miliseconds;
                return true;
            }
            else
            {
                if (rethrottle) throttlers[name] = Environment.TickCount64 + miliseconds;
                return false;
            }
        }

        public bool Check(T name)
        {
            if (!throttlers.ContainsKey(name)) return true;
            return Environment.TickCount64 > throttlers[name];
        }

        public long GetRemainingTime(T name, bool allowNegative = false)
        {
            if (!throttlers.ContainsKey(name)) return allowNegative ? -Environment.TickCount64 : 0;
            var ret = throttlers[name] - Environment.TickCount64;
            if (allowNegative)
            {
                return ret;
            }
            else
            {
                return ret > 0 ? ret : 0;
            }
        }

        public void ImGuiPrintDebugInfo()
        {
            foreach (var x in throttlers)
            {
                ImGuiEx.Text(Check(x.Key) ? ImGuiColors.HealerGreen : ImGuiColors.DalamudRed, $"{x.Key}: [{GetRemainingTime(x.Key)}ms remains] ({x.Value})");
            }
        }
    }
}
