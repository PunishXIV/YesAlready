using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommons.ImGuiMethods
{
    public static class ImGuiNoRef
    {
        public static bool Checkbox(string label, bool value, out bool newResult)
        {
            if(ImGui.Checkbox(label, ref value))
            {
                newResult = value; 
                return true;
            }
            newResult = default;
            return false;
        }
    }
}
