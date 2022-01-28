using Dalamud.Interface;
using ImGuiNET;

namespace YesAlready.Interface
{
    /// <summary>
    /// ImGui wrappers.
    /// </summary>
    internal static class ImGuiEx
    {
        /// <summary>
        /// Create an icon button.
        /// </summary>
        /// <param name="icon">Icon to display.</param>
        /// <returns>A value indicating whether the button has been pressed.</returns>
        public static bool IconButton(FontAwesomeIcon icon) => IconButton(icon);

        /// <summary>
        /// Create an icon button.
        /// </summary>
        /// <param name="icon">Icon to display.</param>
        /// <param name="tooltip">Tooltip to display.</param>
        /// <param name="width">Width of the button.</param>
        /// <returns>A value indicating whether the button has been pressed.</returns>
        public static bool IconButton(FontAwesomeIcon icon, string tooltip, int width = -1)
        {
            ImGui.PushFont(UiBuilder.IconFont);

            if (width > 0)
                ImGui.SetNextItemWidth(32);

            var result = ImGui.Button($"{icon.ToIconString()}##{icon.ToIconString()}-{tooltip}");
            ImGui.PopFont();

            if (tooltip != null)
                TextTooltip(tooltip);

            return result;
        }

        /// <summary>
        /// Gets the width of an icon.
        /// </summary>
        /// <param name="icon">Icon to measure.</param>
        /// <returns>The size of the icon.</returns>
        public static float GetIconWidth(FontAwesomeIcon icon)
        {
            ImGui.PushFont(UiBuilder.IconFont);

            var width = ImGui.CalcTextSize($"{icon.ToIconString()}").X;

            ImGui.PopFont();

            return width;
        }

        /// <summary>
        /// Gets the  width of an icon button.
        /// </summary>
        /// <param name="icon">Icon to measure.</param>
        /// <returns>The size of the icon.</returns>
        public static float GetIconButtonWidth(FontAwesomeIcon icon)
        {
            var style = ImGui.GetStyle();

            return GetIconWidth(icon) + (style.FramePadding.X * 2);
        }

        /// <summary>
        /// Creates a simple text tooltip.
        /// </summary>
        /// <param name="text">Text to display.</param>
        public static void TextTooltip(string text)
        {
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.TextUnformatted(text);
                ImGui.EndTooltip();
            }
        }
    }
}
