using ECommons.ImGuiMethods;
using ImGuiNET;
using System.Numerics;

namespace YesAlready.UI.Components;

public static class NodeStyling
{
    public static void DrawNodeStyle(bool isValid, bool isEnabled)
    {
        if (!isEnabled && !isValid)
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, 0, 0, 1));
        else if (!isEnabled)
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, .5f, .5f, 1));
        else if (!isValid)
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
    }

    public static void DrawNodeTypeBadge(NodeType type)
    {
        var icon = type.GetIcon();
        var color = GetTypeColor(type);
        ImGui.PushStyleColor(ImGuiCol.Text, color);
        ImGuiEx.IconButton(icon, type.GetDisplayName());
        ImGui.PopStyleColor();
    }

    private static Vector4 GetTypeColor(NodeType type) => type switch
    {
        NodeType.YesNo => new Vector4(0.2f, 0.8f, 0.2f, 1.0f),  // Green
        NodeType.Ok => new Vector4(0.2f, 0.6f, 0.8f, 1.0f),    // Blue
        NodeType.List => new Vector4(0.8f, 0.4f, 0.0f, 1.0f),  // Orange
        NodeType.Talk => new Vector4(0.8f, 0.2f, 0.8f, 1.0f),  // Purple
        NodeType.Numerics => new Vector4(0.8f, 0.8f, 0.2f, 1.0f), // Yellow
        _ => new Vector4(0.5f, 0.5f, 0.5f, 1.0f)              // Gray
    };
}
