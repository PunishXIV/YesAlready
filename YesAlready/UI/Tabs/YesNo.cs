using Dalamud.Interface;
using ECommons.DalamudServices;
using ImGuiNET;
using System.Numerics;
using System.Text;


namespace YesAlready.UI.Tabs;
public class YesNo
{
    private static TextFolderNode RootFolder => P.Config.RootFolder;

    public static void DrawButtons()
    {
        var style = ImGui.GetStyle();
        var newStyle = new Vector2(style.ItemSpacing.X / 2, style.ItemSpacing.Y);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newStyle);

        if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.Plus, "Add new entry"))
        {
            var newNode = new TextEntryNode { Enabled = false, Text = "Your text goes here" };
            RootFolder.Children.Add(newNode);
            P.Config.Save();
        }

        ImGui.SameLine();
        if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Add last seen as new entry"))
        {
            var io = ImGui.GetIO();
            var zoneRestricted = io.KeyCtrl;
            var createFolder = io.KeyShift;
            var selectNo = io.KeyAlt;

            Configuration.CreateTextNode(RootFolder, zoneRestricted, createFolder, selectNo);
            P.Config.Save();
        }

        ImGui.SameLine();
        if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
        {
            var newNode = new TextFolderNode { Name = "Untitled folder" };
            RootFolder.Children.Add(newNode);
            P.Config.Save();
        }

        var sb = new StringBuilder();
        sb.AppendLine("Enter into the input all or part of the text inside a dialog.");
        sb.AppendLine("For example: \"Teleport to \" for the teleport dialog.");
        sb.AppendLine();
        sb.AppendLine("Alternatively, wrap your text in forward slashes to use as a regex.");
        sb.AppendLine("As such: \"/Teleport to .*? for \\d+(,\\d+)? gil\\?/\"");
        sb.AppendLine("Or simpler: \"/Teleport to .*?/\" (and hope it doesn't match something unexpected)");
        sb.AppendLine();
        sb.AppendLine("If it matches, the yes button (and checkbox if present) will be clicked.");
        sb.AppendLine();
        sb.AppendLine("Right click a line to view options.");
        sb.AppendLine("Double click an entry for quick enable/disable.");
        sb.AppendLine("Ctrl-Shift right click a line to delete it and any children.");
        sb.AppendLine();
        sb.AppendLine("\"Add last seen as new entry\" button modifiers:");
        sb.AppendLine("   Shift-Click to add to a new or first existing folder with the current zone name, restricted to that zone.");
        sb.AppendLine("   Ctrl-Click to create a entry restricted to the current zone, without a named folder.");
        sb.AppendLine("   Alt-Click to create a \"Select No\" entry instead of \"Select Yes\"");
        sb.AppendLine("   Alt-Click can be combined with Shift/Ctrl-Click.");
        sb.AppendLine();
        sb.AppendLine("Currently supported text addons:");
        sb.AppendLine("  - SelectYesNo");

        ImGui.SameLine();
        Utils.ImGuiEx.IconButton(FontAwesomeIcon.QuestionCircle, sb.ToString());
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(sb.ToString());

        ImGui.SameLine();
        var gimmickConfirm = P.Config.GimmickYesNo;
        if (ImGui.Checkbox("Auto GimmickYesNo", ref gimmickConfirm))
        {
            P.Config.GimmickYesNo = gimmickConfirm;
            P.Config.Save();
        }
        if (ImGui.IsItemHovered()) ImGui.SetTooltip("Automatically confirm any Yesno dialogs that are part of the GimmickYesNo sheet.\nThese are mostly the dungeon Yesnos like \"Unlock this door?\" or \"Pickup this item?\"");

        ImGui.PopStyleVar(); // ItemSpacing
    }

    public static void DisplayEntryNode(TextEntryNode node)
    {
        var validRegex = (node.IsTextRegex && node.TextRegex != null) || !node.IsTextRegex;
        var validZone = !node.ZoneRestricted || (node.ZoneIsRegex && node.ZoneRegex != null) || !node.ZoneIsRegex;

        if (!node.Enabled && (!validRegex || !validZone))
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, 0, 0, 1));
        else if (!node.Enabled)
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, .5f, .5f, 1));
        else if (!validRegex || !validZone)
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));

        ImGui.TreeNodeEx($"{node.Name}##{node.Name}-tree", ImGuiTreeNodeFlags.Leaf);
        ImGui.TreePop();

        if (!node.Enabled || !validRegex || !validZone)
            ImGui.PopStyleColor();

        if (!validRegex && !validZone)
            Utils.ImGuiEx.TextTooltip("Invalid Text and Zone Regex");
        else if (!validRegex)
            Utils.ImGuiEx.TextTooltip("Invalid Text Regex");
        else if (!validZone)
            Utils.ImGuiEx.TextTooltip("Invalid Zone Regex");

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

    public static void DrawPopup(TextEntryNode textNode, Vector2 spacing)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, spacing);

        var enabled = textNode.Enabled;
        if (ImGui.Checkbox("Enabled", ref enabled))
        {
            textNode.Enabled = enabled;
            P.Config.Save();
        }

        ImGui.SameLine(100f);
        var isYes = textNode.IsYes;
        var title = isYes ? "Click Yes" : "Click No";
        if (ImGui.Button(title))
        {
            textNode.IsYes = !isYes;
            P.Config.Save();
        }

        var trashAltWidth = Utils.ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.TrashAlt);

        ImGui.SameLine(ImGui.GetContentRegionMax().X - trashAltWidth);
        if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt, "Delete"))
        {
            if (P.Config.TryFindParent(textNode, out var parentNode))
            {
                parentNode!.Children.Remove(textNode);
                P.Config.Save();
            }
        }

        var matchText = textNode.Text;
        if (ImGui.InputText($"##{textNode.Name}-matchText", ref matchText, 10_000, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
        {
            textNode.Text = matchText;
            P.Config.Save();
        }

        var zoneRestricted = textNode.ZoneRestricted;
        if (ImGui.Checkbox("Zone Restricted", ref zoneRestricted))
        {
            textNode.ZoneRestricted = zoneRestricted;
            P.Config.Save();
        }

        var searchWidth = Utils.ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.Search);
        var searchPlusWidth = Utils.ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.SearchPlus);

        ImGui.SameLine(ImGui.GetContentRegionMax().X - searchWidth);
        if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.Search, "Zone List"))
        {
            P.OpenZoneListUi();
        }

        ImGui.SameLine(ImGui.GetContentRegionMax().X - searchWidth - searchPlusWidth - spacing.X);
        if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Fill with current zone"))
        {
            var currentID = Svc.ClientState.TerritoryType;
            if (P.TerritoryNames.TryGetValue(currentID, out var zoneName))
            {
                textNode.ZoneText = zoneName;
                P.Config.Save();
            }
            else
            {
                textNode.ZoneText = "Could not find name";
                P.Config.Save();
            }
        }

        ImGui.PopStyleVar(); // ItemSpacing

        var zoneText = textNode.ZoneText;
        if (ImGui.InputText($"##{textNode.Name}-zoneText", ref zoneText, 10_000, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
        {
            textNode.ZoneText = zoneText;
            P.Config.Save();
        }

        ImGui.NewLine();

        var conditional = textNode.IsConditional;
        if (ImGui.Checkbox("Is Conditional", ref conditional))
        {
            textNode.IsConditional = conditional;
            P.Config.Save();
        }

        ImGui.Text("Currently only supports number extraction");

        var conditionalText = textNode.ConditionalNumberTemplate;
        if (ImGui.InputText($"##{textNode.Name}-conditionalText", ref conditionalText, 10_000, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
        {
            textNode.ConditionalNumberTemplate = conditionalText;
            P.Config.Save();
        }

        var comparisonType = textNode.ComparisonType;
        if (ImGui.BeginCombo($"##{textNode.Name}-comparisonType", MainWindow.ComparisonTypeToText(comparisonType)))
        {
            foreach (var c in MainWindow.ComparisonTypes)
            {
                var isSelected = comparisonType == c;
                if (ImGui.Selectable(MainWindow.ComparisonTypeToText(c), isSelected))
                {
                    textNode.ComparisonType = c;
                    P.Config.Save();
                }

                if (isSelected)
                    ImGui.SetItemDefaultFocus();
            }
            ImGui.EndCombo();
        }

        var conditionalNumber = textNode.ConditionalNumber;
        if (ImGui.InputInt($"##{textNode.Name}-conditionalNumber", ref conditionalNumber, 1, 10, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
        {
            textNode.ConditionalNumber = conditionalNumber;
            P.Config.Save();
        }
    }
}
