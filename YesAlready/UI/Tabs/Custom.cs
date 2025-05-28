using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using System.Numerics;
using System.Text;
using YesAlready.Features;

namespace YesAlready.UI.Tabs;
public static class Custom
{
    public static void DrawButtons()
    {
        var style = ImGui.GetStyle();
        var newStyle = new Vector2(style.ItemSpacing.X / 2, style.ItemSpacing.Y);
        using var _ = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, newStyle);

        if (ImGuiX.IconButton(FontAwesomeIcon.Plus, "Add new entry"))
        {
            var newNode = new CustomEntryNode
            {
                Enabled = true,
                Addon = "AddonName",
                CallbackParams = "-1"
            };
            C.CustomRootFolder.Children.Add(newNode);
            C.Save();
            CustomAddonCallbacks.Toggle();
        }

        var sb = new StringBuilder();
        sb.AppendLine("This section allows you to build custom \"Bothers\".");
        sb.AppendLine("Many bothers are very simple, consisting of a single callback parameter to a given addon when it appears. This is for those types of bothers.");
        sb.AppendLine();
        sb.AppendLine("Callback parameter parsing works the same as in Something Need Doing.");
        sb.AppendLine("Custom bothers are registered via AddonLifeCycle on the PostSetup event.");
        sb.AppendLine();
        sb.AppendLine("Some bothers may require infeasible parameters, waits, or different AddonEvents. Those can still be requested for the normal bother system.");
        sb.AppendLine();
        sb.AppendLine("Example:");
        sb.AppendLine("   AddonName: Character");
        sb.AppendLine("   Parameters: -1");
        sb.AppendLine("   Effect: When opening the Character addon, it will instantly be closed. Probably not useful.");

        ImGui.SameLine();
        ImGuiX.IconButton(FontAwesomeIcon.QuestionCircle, sb.ToString());
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(sb.ToString());
    }

    public static void DrawPopup(CustomEntryNode node, Vector2 spacing)
    {
        using var _ = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        var enabled = node.Enabled;
        if (ImGui.Checkbox("Enabled", ref enabled))
        {
            node.Enabled = enabled;
            C.Save();
            CustomAddonCallbacks.Toggle();
        }

        var trashAltWidth = ImGuiX.GetIconButtonWidth(FontAwesomeIcon.TrashAlt);

        ImGui.SameLine(ImGui.GetContentRegionMax().X - trashAltWidth);
        if (ImGuiX.IconButton(FontAwesomeIcon.TrashAlt, "Delete"))
        {
            if (C.TryFindParent(node, out var parentNode))
            {
                parentNode!.Children.Remove(node);
                C.Save();
                CustomAddonCallbacks.Toggle();
            }
        }

        ImGui.TextUnformatted("Note:");
        var noteText = node.Text;
        if (ImGui.InputText($"##{node.Name}-{nameof(noteText)}", ref noteText, 10_000, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
        {
            node.Text = noteText;
            C.Save();
        }
        ImGui.SameLine();
        ImGuiComponents.HelpMarker("This is not used for anything, it's just a note to help remember what this bother does.");

        ImGui.TextUnformatted("Addon Name:");
        var addonName = node.Addon;
        if (ImGui.InputText($"##{node.Name}-{nameof(addonName)}", ref addonName, 100, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
        {
            node.Addon = addonName;
            C.Save();
            CustomAddonCallbacks.Toggle();
        }

        ImGui.TextUnformatted("Parameters:");
        var callbackParams = node.CallbackParams;
        if (ImGui.InputText($"##{node.Name}-{nameof(callbackParams)}", ref callbackParams, 150, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
        {
            node.CallbackParams = callbackParams;
            C.Save();
        }
    }
}
