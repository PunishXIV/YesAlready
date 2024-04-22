using Dalamud.Interface;
using ImGuiNET;
using System.Numerics;
using System.Text;

namespace YesAlready.UI.Tabs;
public static class Numerics
{
    private static TextFolderNode NumericsRootFolder => P.Config.NumericsRootFolder;

    public static void DrawButtons()
    {
        var style = ImGui.GetStyle();
        var newStyle = new Vector2(style.ItemSpacing.X / 2, style.ItemSpacing.Y);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newStyle);

        if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.Plus, "Add new entry"))
        {
            var newNode = new NumericsEntryNode { Enabled = false, Text = "Your text goes here" };
            NumericsRootFolder.Children.Add(newNode);
            P.Config.Save();
        }

        ImGui.SameLine();
        if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Add last seen as new entry"))
        {
            var io = ImGui.GetIO();
            var createFolder = io.KeyShift;

            Configuration.CreateNumericsNode(NumericsRootFolder, createFolder);
            P.Config.Save();
        }

        ImGui.SameLine();
        if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
        {
            var newNode = new TextFolderNode { Name = "Untitled folder" };
            NumericsRootFolder.Children.Add(newNode);
            P.Config.Save();
        }

        var sb = new StringBuilder();
        sb.AppendLine("Enter into the input all or part of the text inside a dialog.");
        sb.AppendLine("For example: \"Remove how many from stack?\" for the split stack dialog.");
        sb.AppendLine();
        sb.AppendLine("Alternatively, wrap your text in forward slashes to use as a regex.");
        sb.AppendLine("As such: \"/Remove .*/\"");
        sb.AppendLine();
        sb.AppendLine("If it matches, the ok button will be clicked.");
        sb.AppendLine();
        sb.AppendLine("Right click a line to view options.");
        sb.AppendLine("Double click an entry for quick enable/disable.");
        sb.AppendLine("Ctrl-Shift right click a line to delete it and any children.");
        sb.AppendLine();
        sb.AppendLine("\"Add last seen as new entry\" button modifiers:");
        sb.AppendLine("   Shift-Click to add to a new or first existing folder.");
        sb.AppendLine();
        sb.AppendLine("Currently supported numeric addons:");
        sb.AppendLine("  - InputNumeric");

        ImGui.SameLine();
        Utils.ImGuiEx.IconButton(FontAwesomeIcon.QuestionCircle, sb.ToString());
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(sb.ToString());

        ImGui.PopStyleVar(); // ItemSpacing
    }

    public static void DrawPopup(NumericsEntryNode node, Vector2 spacing)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, spacing);

        var enabled = node.Enabled;
        if (ImGui.Checkbox("Enabled", ref enabled))
        {
            node.Enabled = enabled;
            P.Config.Save();
        }

        var trashAltWidth = Utils.ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.TrashAlt);

        ImGui.SameLine(ImGui.GetContentRegionMax().X - trashAltWidth);
        if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt, "Delete"))
        {
            if (P.Config.TryFindParent(node, out var parentNode))
            {
                parentNode!.Children.Remove(node);
                P.Config.Save();
            }
        }

        var matchText = node.Text;
        if (ImGui.InputText($"##{node.Name}-matchText", ref matchText, 10_000, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
        {
            node.Text = matchText;
            P.Config.Save();
        }

        ImGui.PopStyleVar(); // ItemSpacing

        ImGui.NewLine();

        var percent = node.IsPercent;
        if (ImGui.Checkbox("Percentage", ref percent))
        {
            node.IsPercent = percent;
            P.Config.Save();
        }
        if (node.IsPercent)
        {
            var percentage = node.Percentage;
            if (ImGui.SliderInt($"Percent of Max##{node.GetHashCode()}", ref percentage, 0, 100, "%d%%", ImGuiSliderFlags.AlwaysClamp))
            {
                if (percentage < 0) node.Percentage = 0;
                else node.Percentage = percentage;
                if (percentage > 100) node.Percentage = 100;
                else node.Percentage = percentage;
                P.Config.Save();
            }
        }
        else
        {
            var quantity = node.Quantity;
            if (ImGui.InputInt($"Default Quantity##{node.GetHashCode()}", ref quantity))
            {
                if (quantity < 1) node.Quantity = 1;
                else node.Quantity = quantity;
                P.Config.Save();
            }
        }
    }
}
