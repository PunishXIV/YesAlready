using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using YesAlready.UI.Components;

namespace YesAlready.UI.Tabs;

public class CoreTab : BaseTab
{
    public enum ViewMode
    {
        ByType,
        Alphabetical,
        Folders
    }

    public ViewMode CurrentViewMode { get; private set; } = ViewMode.ByType;

    private string searchFilter = "";
    private bool showDisabled = true;
    private bool showEnabled = true;
    private bool showInvalid = true;

    protected override string TabName => "Core";
    protected override string HelpText => GetHelpText();

    protected override void DrawContent()
    {
        DrawToolbar();
        DrawViewOptions();
        DrawNodeList();
    }

    private void DrawToolbar()
    {
        var style = ImGui.GetStyle();
        var newStyle = new Vector2(style.ItemSpacing.X / 2, style.ItemSpacing.Y);
        using var _ = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, newStyle);

        // Add new entry button with type selection
        if (ImGui.Button("Add New"))
        {
            ImGui.OpenPopup("AddNewEntry");
        }
        DrawAddNewEntryPopup();

        // Add last seen button with preview
        ImGui.SameLine();
        if (ImGui.Button("Add Last Seen"))
        {
            ImGui.OpenPopup("AddLastSeen");
        }
        DrawAddLastSeenPopup();

        // Add folder button
        ImGui.SameLine();
        if (ImGui.Button("Add Folder"))
        {

        }

        // Search filter
        ImGui.SameLine();
        ImGui.SetNextItemWidth(200);
        if (ImGui.InputText("##Search", ref searchFilter, 100))
        {
            // Update filtered results
        }

        // Filter toggles
        ImGui.SameLine();
        if (ImGui.Checkbox("Show Enabled", ref showEnabled)) { }
        ImGui.SameLine();
        if (ImGui.Checkbox("Show Disabled", ref showDisabled)) { }
        ImGui.SameLine();
        if (ImGui.Checkbox("Show Invalid", ref showInvalid)) { }

        DrawHelpButton();
    }

    private void DrawViewOptions()
    {
        ImGui.SameLine();
        ImGui.SetNextItemWidth(150);
        if (ImGui.BeginCombo("##ViewMode", CurrentViewMode.ToString()))
        {
            foreach (ViewMode mode in Enum.GetValues(typeof(ViewMode)))
            {
                if (ImGui.Selectable(mode.ToString(), CurrentViewMode == mode))
                {
                    CurrentViewMode = mode;
                }
            }
            ImGui.EndCombo();
        }
    }

    private void DrawAddNewEntryPopup()
    {
        if (ImGui.BeginPopup("AddNewEntry"))
        {
            if (ImGui.Selectable("Yes/No Dialog"))
            {
                var newNode = new TextEntryNode { Enabled = false, Text = "Your text goes here" };
                C.RootFolder.Children.Add(newNode);
                C.Save();
            }
            if (ImGui.Selectable("OK Dialog"))
            {
                var newNode = new OkEntryNode { Enabled = false, Text = "Your text goes here" };
                C.OkRootFolder.Children.Add(newNode);
                C.Save();
            }
            if (ImGui.Selectable("List Selection"))
            {
                var newNode = new ListEntryNode { Enabled = false, Text = "Your text goes here" };
                C.ListRootFolder.Children.Add(newNode);
                C.Save();
            }
            if (ImGui.Selectable("Talk Dialog"))
            {
                var newNode = new TalkEntryNode { Enabled = false, TargetText = "Your text goes here" };
                C.TalkRootFolder.Children.Add(newNode);
                C.Save();
            }
            if (ImGui.Selectable("Numeric Input"))
            {
                var newNode = new NumericsEntryNode { Enabled = false, Text = "Your text goes here" };
                C.NumericsRootFolder.Children.Add(newNode);
                C.Save();
            }
            ImGui.EndPopup();
        }
    }

    private void DrawAddLastSeenPopup()
    {
        if (ImGui.BeginPopup("AddLastSeen"))
        {
            ImGui.Text("Select type and preview:");

            if (ImGui.CollapsingHeader("Yes/No Dialogs"))
            {
                if (ImGui.Selectable(Service.Watcher.LastSeenDialogText))
                {
                    var newNode = new TextEntryNode
                    {
                        Enabled = false,
                        Text = Service.Watcher.LastSeenDialogText
                    };
                    C.RootFolder.Children.Add(newNode);
                    C.Save();
                }
            }

            if (ImGui.CollapsingHeader("OK Dialogs"))
            {
                if (ImGui.Selectable(Service.Watcher.LastSeenOkText))
                {
                    var newNode = new OkEntryNode
                    {
                        Enabled = false,
                        Text = Service.Watcher.LastSeenOkText
                    };
                    C.RootFolder.Children.Add(newNode);
                    C.Save();
                }
            }

            if (ImGui.CollapsingHeader("List Dialogs"))
            {
                if (ImGui.Selectable(Service.Watcher.LastSeenListSelection))
                {
                    var newNode = new ListEntryNode
                    {
                        Enabled = false,
                        Text = Service.Watcher.LastSeenListSelection
                    };
                    C.RootFolder.Children.Add(newNode);
                    C.Save();
                }
            }

            if (ImGui.CollapsingHeader("Talk Dialogs"))
            {
                if (ImGui.Selectable(Service.Watcher.LastSeenTalkTarget))
                {
                    var newNode = new TalkEntryNode
                    {
                        Enabled = false,
                        TargetText = Service.Watcher.LastSeenTalkTarget
                    };
                    C.RootFolder.Children.Add(newNode);
                    C.Save();
                }
            }

            if (ImGui.CollapsingHeader("Numeric Dialogs"))
            {
                if (ImGui.Selectable(Service.Watcher.LastSeenNumericsText))
                {
                    var newNode = new NumericsEntryNode
                    {
                        Enabled = false,
                        Text = Service.Watcher.LastSeenNumericsText
                    };
                    C.RootFolder.Children.Add(newNode);
                    C.Save();
                }
            }

            ImGui.EndPopup();
        }
    }

    private void DrawNodeList()
    {
        switch (CurrentViewMode)
        {
            case ViewMode.ByType:
                DrawByTypeView();
                break;
            case ViewMode.Alphabetical:
                DrawAlphabeticalView();
                break;
            case ViewMode.Folders:
                DrawFolderView();
                break;
        }
    }

    private void DrawByTypeView()
    {
        if (ImGui.CollapsingHeader("Yes/No Dialogs", ImGuiTreeNodeFlags.DefaultOpen))
        {
            DisplayNodes(C.RootFolder, () => new TextEntryNode() { Enabled = false, Text = "Add some text here!" });
        }

        if (ImGui.CollapsingHeader("OK Dialogs", ImGuiTreeNodeFlags.DefaultOpen))
        {
            DisplayNodes(C.OkRootFolder, () => new OkEntryNode() { Enabled = false, Text = "Add some text here!" });
        }

        if (ImGui.CollapsingHeader("List Dialogs", ImGuiTreeNodeFlags.DefaultOpen))
        {
            DisplayNodes(C.ListRootFolder, () => new ListEntryNode() { Enabled = false, Text = "Add some text here!" });
        }

        if (ImGui.CollapsingHeader("Talk Dialogs", ImGuiTreeNodeFlags.DefaultOpen))
        {
            DisplayNodes(C.TalkRootFolder, () => new TalkEntryNode { Enabled = false, TargetText = "Your text goes here" });
        }

        if (ImGui.CollapsingHeader("Numeric Dialogs", ImGuiTreeNodeFlags.DefaultOpen))
        {
            DisplayNodes(C.NumericsRootFolder, () => new NumericsEntryNode() { Enabled = false, Text = "Add some text here!" });
        }
    }

    private void DrawAlphabeticalView()
    {
        var nodes = GetAllNodes()
            .OrderBy(n => n.Name)
            .Where(FilterNode);

        foreach (var node in nodes)
        {
            MainWindow.DisplayTextNode(node, C.RootFolder);
        }
    }

    private void DrawFolderView()
    {
        DisplayNodes(C.RootFolder, () => new TextEntryNode() { Enabled = false, Text = "Add some text here!" });
    }

    private void DisplayNodes<T>(TextFolderNode root, Func<T> createNewNode) where T : ITextNode
    {
        MainWindow.TextNodeDragDrop(root);

        if (root.Children.Count == 0)
        {
            root.Children.Add(createNewNode());
            C.Save();
        }

        foreach (var node in root.Children.ToArray())
            MainWindow.DisplayTextNode(node, root);
    }

    private IEnumerable<ITextNode> GetAllNodes()
    {
        return C.RootFolder.Children
            .SelectMany(GetAllNodesRecursive);
    }

    private IEnumerable<ITextNode> GetAllNodesRecursive(ITextNode node)
    {
        return node is TextFolderNode folder ? folder.Children.SelectMany(GetAllNodesRecursive) : [node];
    }

    private bool FilterNode(ITextNode node)
    {
        if (!showEnabled && node.Enabled) return false;
        if (!showDisabled && !node.Enabled) return false;
        if (!showInvalid && node is IValidatable validatable && !validatable.IsValid) return false;
        if (!string.IsNullOrEmpty(searchFilter))
        {
            return node.Name.Contains(searchFilter, StringComparison.OrdinalIgnoreCase);
        }
        return true;
    }

    private string GetHelpText()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Core features for YesAlready:");
        sb.AppendLine();
        sb.AppendLine("View Modes:");
        sb.AppendLine("  - By Type: Groups entries by their dialog type");
        sb.AppendLine("  - Alphabetical: Simple alphabetical list");
        sb.AppendLine("  - Folders: Current folder structure");
        sb.AppendLine();
        sb.AppendLine("Features:");
        sb.AppendLine("  - Add new entry: Create a new entry of any type");
        sb.AppendLine("  - Add last seen: Add from recently seen dialogs");
        sb.AppendLine("  - Search: Filter entries by name or text");
        sb.AppendLine("  - Show/Hide: Toggle visibility of enabled/disabled/invalid entries");
        return sb.ToString();
    }
}
