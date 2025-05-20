using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using ECommons.ImGuiMethods;
using ImGuiNET;

namespace YesAlready.UI.Components;

public abstract class BaseTab
{
    protected abstract string TabName { get; }
    protected abstract string HelpText { get; }

    public virtual void Draw()
    {
        using var tab = ImRaii.TabItem(TabName);
        if (!tab) return;
        using var idScope = ImRaii.PushId($"{TabName}Options");

        DrawContent();
    }

    protected abstract void DrawContent();

    protected virtual void DrawHelpButton()
    {
        ImGui.SameLine();
        ImGuiEx.IconButton(FontAwesomeIcon.QuestionCircle, HelpText);
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(HelpText);
    }
}
