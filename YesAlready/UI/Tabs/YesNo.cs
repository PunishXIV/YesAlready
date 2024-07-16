using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using System.Numerics;
using System.Text;
using YesAlready.Utils;

namespace YesAlready.UI.Tabs;
public class YesNo
{
    private static TextFolderNode RootFolder => P.Config.RootFolder;

    public static void DrawButtons()
    {
        var style = ImGui.GetStyle();
        var newStyle = new Vector2(style.ItemSpacing.X / 2, style.ItemSpacing.Y);
        using var _ = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, newStyle);

        if (ImGuiEx.IconButton(FontAwesomeIcon.Plus, "Add new entry"))
        {
            var newNode = new TextEntryNode { Enabled = false, Text = "Your text goes here" };
            RootFolder.Children.Add(newNode);
            P.Config.Save();
        }

        ImGui.SameLine();
        if (ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Add last seen as new entry"))
        {
            var io = ImGui.GetIO();
            var zoneRestricted = io.KeyCtrl;
            var createFolder = io.KeyShift;
            var selectNo = io.KeyAlt;

            Configuration.CreateTextNode(RootFolder, zoneRestricted, createFolder, selectNo);
            P.Config.Save();
        }

        ImGui.SameLine();
        if (ImGuiEx.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
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
        ImGuiEx.IconButton(FontAwesomeIcon.QuestionCircle, sb.ToString());
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(sb.ToString());

        ImGui.SameLine();
        if (ImGuiComponents.IconButton(FontAwesomeIcon.EllipsisH))
            ImGui.OpenPopup("SelectYesno additional options");
        DrawYesnoBothers();
    }

    private static void DrawYesnoBothers()
    {
        using var popup = ImRaii.Popup("SelectYesno additional options");
        if (popup.Success)
        {
            var gimmickConfirm = P.Config.GimmickYesNo;
            if (ImGui.Checkbox("Auto GimmickYesNo", ref gimmickConfirm))
            {
                P.Config.GimmickYesNo = gimmickConfirm;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Automatically confirm any Yesno dialogs that are part of the GimmickYesNo sheet.\nThese are mostly the dungeon Yesnos like \"Unlock this door?\" or \"Pickup this item?\"", wrapped: false);

            var pfConfirm = P.Config.PartyFinderJoinConfirm;
            if (ImGui.Checkbox("LookingForGroup x SelectYesno", ref pfConfirm))
            {
                P.Config.PartyFinderJoinConfirm = pfConfirm;
                P.Config.Save();
            }

            ImGuiEx.IndentedTextColored("Automatically confirm when joining a party finder group.", wrapped: false);

            var autoCollect = P.Config.AutoCollectable;
            if (ImGui.Checkbox("Auto Collectables", ref autoCollect))
            {
                P.Config.AutoCollectable = autoCollect;
                P.Config.Save();
            }

            ImGuiEx.IndentedTextColored("Automatically accept collectables that are worth turning in and decline insufficient ones.", wrapped: false);
        }
    }

    public static void DisplayEntryNode(TextEntryNode node)
    {
        var validRegex = node.IsTextRegex && node.TextRegex != null || !node.IsTextRegex;
        var validZone = !node.ZoneRestricted || node.ZoneIsRegex && node.ZoneRegex != null || !node.ZoneIsRegex;

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
            ImGuiEx.TextTooltip("Invalid Text and Zone Regex");
        else if (!validRegex)
            ImGuiEx.TextTooltip("Invalid Text Regex");
        else if (!validZone)
            ImGuiEx.TextTooltip("Invalid Zone Regex");

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
        using var _ = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);

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

        var trashAltWidth = ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.TrashAlt);

        ImGui.SameLine(ImGui.GetContentRegionMax().X - trashAltWidth);
        if (ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt, "Delete"))
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

        var searchWidth = ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.Search);
        var searchPlusWidth = ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.SearchPlus);

        ImGui.SameLine(ImGui.GetContentRegionMax().X - searchWidth);
        if (ImGuiEx.IconButton(FontAwesomeIcon.Search, "Zone List"))
            P.OpenZoneListUi();

        ImGui.SameLine(ImGui.GetContentRegionMax().X - searchWidth - searchPlusWidth - spacing.X);
        if (ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Fill with current zone"))
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

        var zoneText = textNode.ZoneText;
        if (ImGui.InputText($"##{textNode.Name}-zoneText", ref zoneText, 10_000, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
        {
            textNode.ZoneText = zoneText;
            P.Config.Save();
        }

        var conditionRestricted = textNode.RequiresPlayerConditions;
        if (ImGui.Checkbox("Condition Restricted", ref conditionRestricted))
        {
            textNode.RequiresPlayerConditions = conditionRestricted;
            P.Config.Save();
        }
        ImGuiComponents.HelpMarker($"Conditions can either be their name (case sensitive) or ID. They must be comma separated if there are multiple. Condition restricted only allows the match to go through if all conditions are met. If you would like to invert a condition, put a \"!\" in front of it.");

        ImGui.SameLine(ImGui.GetContentRegionMax().X - searchWidth);
        if (ImGuiEx.IconButton(FontAwesomeIcon.Search, "Conditions List"))
            P.OpenConditionsListUi();

        var playerConditions = textNode.PlayerConditions;
        if (ImGui.InputText($"##{textNode.Name}-playerConditionsText", ref playerConditions, 10_000, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
        {
            textNode.PlayerConditions = playerConditions;
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
