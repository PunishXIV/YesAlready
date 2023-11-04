using Dalamud.Interface.Colors;
using ECommons.DalamudServices;
using ECommons.ImGuiMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommons.Throttlers
{
    public class FrameThrottler<T>
    {
        Dictionary<T, long> throttlers = new();
        long SFrameCount => (long)Svc.PluginInterface.UiBuilder.FrameCount;

        public IReadOnlyCollection<T> ThrottleNames => throttlers.Keys;

        public bool Throttle(T name, int frames = 60, bool rethrottle = false)
        {
            if (!throttlers.ContainsKey(name))
            {
                throttlers[name] = SFrameCount + frames;
                return true;
            }
            if (SFrameCount > throttlers[name])
            {
                throttlers[name] = SFrameCount + frames;
                return true;
            }
            else
            {
                if (rethrottle) throttlers[name] = SFrameCount + frames;
                return false;
            }
        }

        public bool Check(T name)
        {
            if (!throttlers.ContainsKey(name)) return true;
            return SFrameCount > throttlers[name];
        }

        public long GetRemainingTime(T name, bool allowNegative = false)
        {
            if (!throttlers.ContainsKey(name)) return allowNegative ? -SFrameCount : 0;
            var ret = throttlers[name] - SFrameCount;
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
                ImGuiEx.Text(Check(x.Key) ? ImGuiColors.HealerGreen : ImGuiColors.DalamudRed, $"{x.Key}: [{GetRemainingTime(x.Key)} frames remains] ({x.Value})");
            }
        }
    }
}
