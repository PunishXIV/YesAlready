using Dalamud.Interface;
using ImGuiNET;

namespace YesAlready.Utils;

internal static class ImGuiEx
{
    public static bool IconButton(FontAwesomeIcon icon) => IconButton(icon);

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

    public static float GetIconWidth(FontAwesomeIcon icon)
    {
        ImGui.PushFont(UiBuilder.IconFont);

        var width = ImGui.CalcTextSize($"{icon.ToIconString()}").X;

        ImGui.PopFont();

        return width;
    }

    public static float GetIconButtonWidth(FontAwesomeIcon icon)
    {
        var style = ImGui.GetStyle();

        return GetIconWidth(icon) + (style.FramePadding.X * 2);
    }

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
