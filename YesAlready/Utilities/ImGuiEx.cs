using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using System.Numerics;

namespace YesAlready.Utils;

internal static class ImGuiEx
{
    private static Vector4 shadedColor = new(0.68f, 0.68f, 0.68f, 1.0f);

    public static void IndentedTextColored(string text, Vector4 color = default, bool wrapped = true)
    {
        var width = 27f * ImGuiHelpers.GlobalScale;
        color = color == default ? shadedColor : color;
        using var indent = ImRaii.PushIndent(width);
        using var colour = ImRaii.PushColor(ImGuiCol.Text, color);
        if (wrapped)
            ImGui.TextWrapped(text);
        else
            ImGui.TextUnformatted(text);
    }

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

        return GetIconWidth(icon) + style.FramePadding.X * 2;
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
