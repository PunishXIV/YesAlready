using Dalamud.Interface;
using ImGuiNET;
using System.Numerics;
using System.Text;

namespace YesAlready.UI.Tabs;
public static class Talk
{
    private static TextFolderNode TalkRootFolder => P.Config.TalkRootFolder;

    public static void DrawButtons()
    {
        var style = ImGui.GetStyle();
        var newStyle = new Vector2(style.ItemSpacing.X / 2, style.ItemSpacing.Y);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newStyle);

        if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.Plus, "Add new entry"))
        {
            var newNode = new TalkEntryNode { Enabled = false, TargetText = "Your text goes here" };
            TalkRootFolder.Children.Add(newNode);
            P.Config.Save();
        }

        ImGui.SameLine();
        if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Add current target as a new entry"))
        {
            var target = Svc.Targets.Target;
            if (target != null)
            {
                var targetName = P.LastSeenTalkTarget = Utils.SEString.GetSeStringText(target.Name);
                var newNode = new TalkEntryNode { Enabled = true, TargetText = targetName };
                TalkRootFolder.Children.Add(newNode);
                P.Config.Save();
            }
            else
                Svc.Toasts.ShowError("Unable to add entry: no target selected.");
        }

        ImGui.SameLine();
        if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
        {
            var newNode = new TextFolderNode { Name = "Untitled folder" };
            TalkRootFolder.Children.Add(newNode);
            P.Config.Save();
        }

        var sb = new StringBuilder();
        sb.AppendLine("Enter into the input all or part of the selected taret name while in a talk dialog.");
        sb.AppendLine("For example: \"Moyce\" in the Crystarium.");
        sb.AppendLine();
        sb.AppendLine("Alternatively, wrap your text in forward slashes to use as a regex.");
        sb.AppendLine("As such: \"/(Moyce|Eirikur)/\"");
        sb.AppendLine();
        sb.AppendLine("To skip your retainers, add the summoning bell.");
        sb.AppendLine();
        sb.AppendLine("Right click a line to view options.");
        sb.AppendLine("Double click an entry for quick enable/disable.");
        sb.AppendLine("Ctrl-Shift right click a line to delete it and any children.");
        sb.AppendLine();
        sb.AppendLine("Currently supported list addons:");
        sb.AppendLine("  - Talk");

        ImGui.SameLine();
        Utils.ImGuiEx.IconButton(FontAwesomeIcon.QuestionCircle, sb.ToString());

        ImGui.PopStyleVar(); // ItemSpacing
    }

    public static void DisplayTalkEntryNode(TalkEntryNode node)
    {
        var validTarget = node.TargetIsRegex && node.TargetRegex != null || !node.TargetIsRegex;

        if (!node.Enabled && !validTarget)
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, 0, 0, 1));
        else if (!node.Enabled)
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, .5f, .5f, 1));
        else if (!validTarget)
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));

        ImGui.TreeNodeEx($"{node.Name}##{node.Name}-tree", ImGuiTreeNodeFlags.Leaf);
        ImGui.TreePop();

        if (!node.Enabled || !validTarget)
            ImGui.PopStyleColor();

        if (!validTarget)
            Utils.ImGuiEx.TextTooltip("Invalid Target Regex");

        if (ImGui.IsItemHovered())
        {
            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                node.Enabled = !node.Enabled;
                P.Config.Save();
                return;
            }
            else if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
            {
                var io = ImGui.GetIO();
                if (io.KeyCtrl && io.KeyShift)
                {
                    if (P.Config.TryFindParent(node, out var parent))
                    {
                        parent!.Children.Remove(node);
                        P.Config.Save();
                    }

                    return;
                }
                else
                {
                    ImGui.OpenPopup($"{node.GetHashCode()}-popup");
                }
            }
        }

        MainWindow.TextNodePopup(node);
        MainWindow.TextNodeDragDrop(node);
    }

    public static void DrawPopup(TalkEntryNode node, Vector2 spacing)
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

        var searchPlusWidth = Utils.ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.SearchPlus);

        ImGui.SameLine(ImGui.GetContentRegionMax().X - searchPlusWidth - trashAltWidth - spacing.X);
        if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Fill with current target"))
        {
            var target = Svc.Targets.Target;
            var name = target?.Name?.TextValue ?? string.Empty;

            if (!string.IsNullOrEmpty(name))
            {
                node.TargetText = name;
                P.Config.Save();
            }
            else
            {
                node.TargetText = "Could not find target";
                P.Config.Save();
            }
        }

        ImGui.PopStyleVar(); // ItemSpacing

        var targetText = node.TargetText;
        if (ImGui.InputText($"##{node.Name}-targetText", ref targetText, 10_000, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
        {
            node.TargetText = targetText;
            P.Config.Save();
        }
    }
}
