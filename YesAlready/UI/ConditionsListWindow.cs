using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Linq;
using System.Numerics;

namespace YesAlready.Interface;

internal class ConditionsListWindow : Window
{
    public static string Title = $"{Name} Conditions List";
    public ConditionsListWindow() : base(Title)
    {
        Size = new Vector2(525, 600);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw()
    {
        using var _ = ImRaii.PushColor(ImGuiCol.ResizeGrip, 0);

        ImGui.TextUnformatted($"Current Conditions: {string.Join(", ", Svc.Condition.AsReadOnlySet().Where(x => Svc.Condition[x]).Select(flag => flag.ToString()).ToList())}");

        ImGui.Columns(2);

        ImGui.TextUnformatted("ID");
        ImGui.NextColumn();

        ImGui.TextUnformatted("Name");
        ImGui.NextColumn();

        ImGui.Separator();

        foreach (var flag in Enum.GetValues<ConditionFlag>())
        {
            ImGui.TextUnformatted($"{flag}");
            ImGui.NextColumn();

            ImGui.TextUnformatted($"{(int)Enum.Parse<ConditionFlag>(flag.ToString())}");
            ImGui.NextColumn();
        }

        ImGui.Columns(1);
    }
}
