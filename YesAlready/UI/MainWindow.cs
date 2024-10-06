using Dalamud.Game.Text;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ECommons.Logging;
using ECommons.Reflection;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using YesAlready.UI.Tabs;
using YesAlready.Utils;

namespace YesAlready.UI;

internal class MainWindow : Window
{
    public MainWindow() : base($"{Name} {P.GetType().Assembly.GetName().Version}###{Name}")
    {
        Size = new Vector2(525, 600);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public static readonly ComparisonType[] ComparisonTypes =
    [
        ComparisonType.LessThan,
        ComparisonType.LessThanOrEqual,
        ComparisonType.GreaterThan,
        ComparisonType.GreaterThanOrEqual,
    ];

    private static ITextNode? DraggedNode;

    private static TextFolderNode YesNoRootFolder => P.Config.RootFolder;
    private static TextFolderNode OkRootFolder => P.Config.OkRootFolder;
    private static TextFolderNode ListRootFolder => P.Config.ListRootFolder;
    private static TextFolderNode TalkRootFolder => P.Config.TalkRootFolder;
    private static TextFolderNode NumericsRootFolder => P.Config.NumericsRootFolder;

    public override void PreDraw() => ImGui.PushStyleColor(ImGuiCol.ResizeGrip, 0);
    public override void PostDraw() => ImGui.PopStyleColor();

    public override void Draw()
    {
        if (P.BlockListHandler.Locked)
        {
            ECommons.ImGuiMethods.ImGuiEx.TextWrapped(ImGuiColors.DalamudRed, $"Yes Already function is paused because following plugins have requested it: {P.BlockListHandler.BlockList.Print()}");
            if (ImGui.Button("Force unlock"))
            {
                P.BlockListHandler.BlockList.Clear();
            }
        }

        var enabled = P.Config.Enabled;
        if (ImGui.Checkbox("Enabled", ref enabled))
        {
            P.Config.Enabled = enabled;
            P.Config.Save();
        }

        using var tabs = ImRaii.TabBar("Tabs");
        if (tabs)
        {
            DisplayGenericOptions("YesNo", YesNo.DrawButtons, () => DisplayNodes(YesNoRootFolder, () => new TextEntryNode() { Enabled = false, Text = "Add some text here!" }));
            DisplayGenericOptions("Ok", Ok.DrawButtons, () => DisplayNodes(OkRootFolder, () => new OkEntryNode() { Enabled = false, Text = "Add some text here!" }));
            DisplayGenericOptions("List", Lists.DrawButtons, () => DisplayNodes(ListRootFolder, () => new ListEntryNode() { Enabled = false, Text = "Add some text here!" }));
            DisplayGenericOptions("Talk", Talk.DrawButtons, () => DisplayNodes(TalkRootFolder, () => new TalkEntryNode { Enabled = false, TargetText = "Your text goes here" }));
            DisplayGenericOptions("Numerics", Numerics.DrawButtons, () => DisplayNodes(NumericsRootFolder, () => new NumericsEntryNode() { Enabled = false, Text = "Add some text here!" }));
            Bothers.Draw();
            Custom.Draw();
            DisplayMiscOptions();
            using (var tab = ImRaii.TabItem("Log"))
                if (tab)
                    InternalLog.PrintImgui();
        }
    }

    // ====================================================================================================

    private void DisplayGenericOptions(string tabName, Action displayButtons, Action displayNodes)
    {
        using var tab = ImRaii.TabItem(tabName);
        if (!tab) return;
        using var idScope = ImRaii.PushId($"{tabName}Options");
        displayButtons();
        displayNodes();
    }

    private readonly XivChatType? selectedChannel;
    private void DisplayMiscOptions()
    {
        using var tab = ImRaii.TabItem("Settings");
        if (!tab) return;
        using (ImRaii.PushId("Server info bar"))
        {
            try
            {
                var config = DalamudReflector.GetService("Dalamud.Configuration.Internal.DalamudConfiguration");
                var dtrList = config.GetFoP<List<string>>("DtrIgnore");
                var enabled = !dtrList.Contains(Svc.PluginInterface.InternalName);
                if (ImGui.Checkbox("DTR", ref enabled))
                {
                    if (enabled)
                    {
                        dtrList.Remove(Svc.PluginInterface.InternalName);
                    }
                    else
                    {
                        dtrList.Add(Svc.PluginInterface.InternalName);
                    }
                    config.Call("QueueSave", []);
                }
                ImGuiEx.IndentedTextColored($"Display the status of the {Name} in the Server Info Bar (DTR Bar). Clicking toggles the plugin.");
            }
            catch (Exception e)
            {
                ECommons.ImGuiMethods.ImGuiEx.TextWrapped(ImGuiColors.DalamudRed, $"{e}");
            }
        }

        using (var combo = ImRaii.Combo("###ChatChannelSelect", $"{Enum.GetName(typeof(XivChatType), P.Config.MessageChannel)}"))
        {
            if (combo)
            {
                foreach (var type in Enum.GetValues<XivChatType>())
                {
                    using (ImRaii.PushId(type.ToString()))
                    {
                        var selected = ImGui.Selectable($"{type}", type == selectedChannel);

                        if (selected)
                        {
                            P.Config.MessageChannel = type;
                            P.Config.Save();
                        }
                    }
                }
            }
        }
        ImGuiEx.IndentedTextColored($"Select the chat channel for {Name} messages to output to.");
    }

    // ====================================================================================================

    private void DisplayNodes<T>(TextFolderNode root, Func<T> createNewNode)
    {
        TextNodeDragDrop(root);

        if (root.Children.Count == 0)
        {
            root.Children.Add((ITextNode)createNewNode());
            P.Config.Save();
        }

        foreach (var node in root.Children.ToArray())
            DisplayTextNode(node, root);
    }

    // ====================================================================================================

    public static void DisplayTextNode(ITextNode node, TextFolderNode rootNode)
    {
        if (node is TextFolderNode folderNode)
            DisplayFolderNode(folderNode, rootNode);
        else if (node is TextEntryNode textNode)
            YesNo.DisplayEntryNode(textNode);
        else if (node is OkEntryNode okNode)
            DisplayEntryNode<OkEntryNode>(okNode);
        else if (node is ListEntryNode listNode)
            DisplayEntryNode<ListEntryNode>(listNode);
        else if (node is TalkEntryNode talkNode)
            DisplayEntryNode<TalkEntryNode>(talkNode);
        else if (node is NumericsEntryNode numNode)
            DisplayEntryNode<NumericsEntryNode>(numNode);
    }

    private static void DisplayEntryNode<T>(ITextNode node)
    {
        var validRegex = node.IsRegex && node.Regex != null || !node.IsRegex;

        if (!node.Enabled && !validRegex)
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, 0, 0, 1));
        else if (!node.Enabled)
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, .5f, .5f, 1));
        else if (!validRegex)
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));

        ImGui.TreeNodeEx($"{node.Name}##{node.Name}-tree", ImGuiTreeNodeFlags.Leaf);
        ImGui.TreePop();

        if (!node.Enabled || !validRegex)
            ImGui.PopStyleColor();

        if (!validRegex)
            ImGuiEx.TextTooltip("Invalid Text Regex");

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

        TextNodePopup(node);
        TextNodeDragDrop(node);
    }

    private static void DisplayFolderNode(TextFolderNode node, TextFolderNode root)
    {
        var expanded = ImGui.TreeNodeEx($"{node.Name}##{node.GetHashCode()}-tree");

        if (ImGui.IsItemHovered())
        {
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
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

        TextNodePopup(node, root);
        TextNodeDragDrop(node);

        if (expanded)
        {
            foreach (var childNode in node.Children.ToArray())
                DisplayTextNode(childNode, root);

            ImGui.TreePop();
        }
    }

    public static void TextNodePopup(ITextNode node, TextFolderNode? root = null)
    {
        var style = ImGui.GetStyle();
        var spacing = new Vector2(style.ItemSpacing.X / 2, style.ItemSpacing.Y);

        if (ImGui.BeginPopup($"{node.GetHashCode()}-popup"))
        {
            if (node is TextEntryNode entryNode)
                YesNo.DrawPopup(entryNode, spacing);

            if (node is OkEntryNode okNode)
                Ok.DrawPopup(okNode, spacing);

            if (node is ListEntryNode listNode)
                Lists.DrawPopup(listNode, spacing);

            if (node is TalkEntryNode talkNode)
                Talk.DrawPopup(talkNode, spacing);

            if (node is NumericsEntryNode numNode)
                Numerics.DrawPopup(numNode, spacing);

            if (node is TextFolderNode folderNode)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, spacing);

                if (ImGuiEx.IconButton(FontAwesomeIcon.Plus, "Add entry"))
                {
                    if (root == YesNoRootFolder)
                    {
                        var newNode = new TextEntryNode { Enabled = false, Text = "Your text goes here" };
                        folderNode.Children.Add(newNode);
                    }
                    else if (root == ListRootFolder)
                    {
                        var newNode = new ListEntryNode { Enabled = false, Text = "Your text goes here" };
                        folderNode.Children.Add(newNode);
                    }

                    P.Config.Save();
                }

                ImGui.SameLine();
                if (ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Add last seen as new entry"))
                {
                    if (root == YesNoRootFolder)
                    {
                        var io = ImGui.GetIO();
                        var zoneRestricted = io.KeyCtrl;
                        var createFolder = io.KeyShift;
                        var selectNo = io.KeyAlt;

                        Configuration.CreateTextNode(folderNode, zoneRestricted, createFolder, selectNo);
                        P.Config.Save();
                    }
                    else if (root == OkRootFolder || root == NumericsRootFolder)
                    {
                        var createFolder = ImGui.GetIO().KeyShift;
                        Configuration.CreateOkNode(folderNode, createFolder);
                        P.Config.Save();
                    }
                    else if (root == ListRootFolder)
                    {
                        var newNode = new ListEntryNode() { Enabled = true, Text = P.LastSeenListSelection, TargetRestricted = true, TargetText = P.LastSeenListTarget };
                        folderNode.Children.Add(newNode);
                        P.Config.Save();
                    }
                    else if (root == TalkRootFolder)
                    {
                        var newNode = new TalkEntryNode() { Enabled = true, TargetText = P.LastSeenTalkTarget };
                        folderNode.Children.Add(newNode);
                        P.Config.Save();
                    }
                }

                ImGui.SameLine();
                if (ImGuiEx.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
                {
                    var newNode = new TextFolderNode { Name = "Untitled folder" };
                    folderNode.Children.Add(newNode);
                    P.Config.Save();
                }

                var trashWidth = ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.TrashAlt);
                ImGui.SameLine(ImGui.GetContentRegionMax().X - trashWidth);
                if (ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt, "Delete"))
                {
                    if (P.Config.TryFindParent(node, out var parentNode))
                    {
                        parentNode!.Children.Remove(node);
                        P.Config.Save();
                    }
                }

                ImGui.PopStyleVar(); // ItemSpacing

                var folderName = folderNode.Name;
                if (ImGui.InputText($"##{node.Name}-rename", ref folderName, 10_000, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    folderNode.Name = folderName;
                    P.Config.Save();
                }
            }

            ImGui.EndPopup();
        }
    }

    public static string ComparisonTypeToText(ComparisonType comparisonType)
    {
        return comparisonType switch
        {
            ComparisonType.LessThan => "Less than",
            ComparisonType.LessThanOrEqual => "Less than or equal",
            ComparisonType.GreaterThan => "Greater than",
            ComparisonType.GreaterThanOrEqual => "Greater than or equal",
            ComparisonType.Equal => "Equal",
            _ => throw new Exception("Invalid enum value"),
        };
    }

    public static void TextNodeDragDrop(ITextNode node)
    {
        if (node != YesNoRootFolder && node != ListRootFolder && node != TalkRootFolder && ImGui.BeginDragDropSource())
        {
            DraggedNode = node;

            ImGui.Text(node.Name);
            ImGui.SetDragDropPayload("TextNodePayload", IntPtr.Zero, 0);
            ImGui.EndDragDropSource();
        }

        if (ImGui.BeginDragDropTarget())
        {
            var payload = ImGui.AcceptDragDropPayload("TextNodePayload");

            bool nullPtr;
            unsafe
            {
                nullPtr = payload.NativePtr == null;
            }

            var targetNode = node;
            if (!nullPtr && payload.IsDelivery() && DraggedNode != null)
            {
                if (P.Config.TryFindParent(DraggedNode, out var draggedNodeParent))
                {
                    if (targetNode is TextFolderNode targetFolderNode)
                    {
                        draggedNodeParent!.Children.Remove(DraggedNode);
                        targetFolderNode.Children.Add(DraggedNode);
                        P.Config.Save();
                    }
                    else
                    {
                        if (P.Config.TryFindParent(targetNode, out var targetNodeParent))
                        {
                            var targetNodeIndex = targetNodeParent!.Children.IndexOf(targetNode);
                            if (targetNodeParent == draggedNodeParent)
                            {
                                var draggedNodeIndex = targetNodeParent.Children.IndexOf(DraggedNode);
                                if (draggedNodeIndex < targetNodeIndex)
                                {
                                    targetNodeIndex -= 1;
                                }
                            }

                            draggedNodeParent!.Children.Remove(DraggedNode);
                            targetNodeParent.Children.Insert(targetNodeIndex, DraggedNode);
                            P.Config.Save();
                        }
                        else
                        {
                            throw new Exception($"Could not find parent of node \"{targetNode.Name}\"");
                        }
                    }
                }
                else
                {
                    throw new Exception($"Could not find parent of node \"{DraggedNode.Name}\"");
                }

                DraggedNode = null;
            }

            ImGui.EndDragDropTarget();
        }
    }
}
