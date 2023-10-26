using System.Linq;
using System.Numerics;

using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using ImGuiNET;

namespace YesAlready.Interface;

internal class ZoneListWindow : Window
{
    private bool sortZoneByName = false;

    public ZoneListWindow()
        : base("Yes Already Zone List")
    {
        this.Size = new Vector2(525, 600);
        this.SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void PreDraw()
    {
        ImGui.PushStyleColor(ImGuiCol.ResizeGrip, 0);
    }

    public override void PostDraw()
    {
        ImGui.PopStyleColor();
    }

    public override void Draw()
    {
        ImGui.Text($"Current ID: {Svc.ClientState.TerritoryType}");

        ImGui.Checkbox("Sort by Name", ref sortZoneByName);

        ImGui.Columns(2);

        ImGui.Text("ID");
        ImGui.NextColumn();

        ImGui.Text("Name");
        ImGui.NextColumn();

        ImGui.Separator();

        var names = P.TerritoryNames.AsEnumerable();

        if (this.sortZoneByName)
            names = names.ToList().OrderBy(kvp => kvp.Value);

        foreach (var kvp in names)
        {
            ImGui.Text($"{kvp.Key}");
            ImGui.NextColumn();

            ImGui.Text($"{kvp.Value}");
            ImGui.NextColumn();
        }

        ImGui.Columns(1);
    }
}
