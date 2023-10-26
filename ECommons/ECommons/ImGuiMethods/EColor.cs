using Dalamud.Interface.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ECommons.ImGuiMethods
{
    /// <summary>
    /// A set of fancy color for use in plugins. You can redefine them to match necessary style!
    /// </summary>
    public static class EColor
    {
        public static Vector4 RedBright = ImGuiEx.Vector4FromRGB(0xFF0000);
        public static Vector4 Red = ImGuiEx.Vector4FromRGB(0xAA0000);
        public static Vector4 GreenBright = ImGuiEx.Vector4FromRGB(0x00ff00);
        public static Vector4 Green = ImGuiEx.Vector4FromRGB(0x00aa00);
        public static Vector4 BlueBright = ImGuiEx.Vector4FromRGB(0x0000ff);
        public static Vector4 Blue = ImGuiEx.Vector4FromRGB(0x0000aa);
        public static Vector4 White = ImGuiEx.Vector4FromRGB(0xFFFFFF);
        public static Vector4 Black = ImGuiEx.Vector4FromRGB(0x000000);
        public static Vector4 YellowBright = ImGuiEx.Vector4FromRGB(0xFFFF00);
        public static Vector4 Yellow = ImGuiEx.Vector4FromRGB(0xAAAA00);
        public static Vector4 OrangeBright = ImGuiEx.Vector4FromRGB(0xFF7F00);
        public static Vector4 Orange = ImGuiEx.Vector4FromRGB(0xAA5400);
        public static Vector4 CyanBright = ImGuiEx.Vector4FromRGB(0x00FFFF);
        public static Vector4 Cya = ImGuiEx.Vector4FromRGB(0x00aaaa);
        public static Vector4 VioletBright = ImGuiEx.Vector4FromRGB(0xFF00FF);
        public static Vector4 Violet = ImGuiEx.Vector4FromRGB(0xAA00AA);
        public static Vector4 BlueSky = ImGuiEx.Vector4FromRGB(0x0085FF);
        public static Vector4 BlueSea = ImGuiEx.Vector4FromRGB(0x0058AA);
        public static Vector4 PurpleBright = ImGuiEx.Vector4FromRGB(0xFF0084);
        public static Vector4 Purple = ImGuiEx.Vector4FromRGB(0xAA0058);
        public static Vector4 PinkLight = ImGuiEx.Vector4FromRGB(0xFFABD6);
    }
}
