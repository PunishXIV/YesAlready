using System.Collections.Generic;
using System.IO;
using System.Linq;

using Dalamud.Configuration;
using Dalamud.Game.ClientState.Keys;
using Newtonsoft.Json;

namespace YesAlready;

/// <summary>
/// Plugin configuration.
/// </summary>
internal partial class YesAlreadyConfiguration : IPluginConfiguration
{
    /// <summary>
    /// Gets or sets the configuration version.
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Gets or sets a value indicating whether the plugin functionality is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the hotkey to always click yes.
    /// </summary>
    public VirtualKey ForcedYesKey { get; set; } = VirtualKey.NO_KEY;

    /// <summary>
    /// Gets or sets the hotkey to disable all functionality.
    /// </summary>
    public VirtualKey DisableKey { get; set; } = VirtualKey.NO_KEY;

    /// <summary>
    /// Gets the text root folder.
    /// </summary>
    public TextFolderNode RootFolder { get; private set; } = new TextFolderNode { Name = "/" };

    /// <summary>
    /// Gets the list root folder.
    /// </summary>
    public TextFolderNode ListRootFolder { get; private set; } = new TextFolderNode { Name = "/" };

    /// <summary>
    /// Gets the talk root folder.
    /// </summary>
    public TextFolderNode TalkRootFolder { get; private set; } = new TextFolderNode { Name = "/" };

    /// <summary>
    /// Gets or sets a value indicating whether the desynth dialog setting is enabled.
    /// </summary>
    public bool DesynthDialogEnabled { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the desynth bulk dialog setting is enabled.
    /// </summary>
    public bool DesynthBulkDialogEnabled { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the materialize dialog setting is enabled.
    /// </summary>
    public bool MaterializeDialogEnabled { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the materia retrieve dialog setting is enabled.
    /// </summary>
    public bool MateriaRetrieveDialogEnabled { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the item inspection result dialog setting is enabled.
    /// </summary>
    public bool ItemInspectionResultEnabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the item inspection result limit, where the inspection loop will pause after that many items.
    /// </summary>
    public int ItemInspectionResultRateLimiter { get; set; } = 0;

    /// <summary>
    /// Gets or sets a value indicating whether the retainer task ask dialog setting is enabled.
    /// </summary>
    public bool RetainerTaskAskEnabled { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the retainer task result dialog setting is enabled.
    /// </summary>
    public bool RetainerTaskResultEnabled { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the grand company supply reward dialog setting is enabled.
    /// </summary>
    public bool GrandCompanySupplyReward { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the shop card dialog setting is enabled.
    /// </summary>
    public bool ShopCardDialog { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the journal result complete setting is enabled.
    /// </summary>
    public bool JournalResultCompleteEnabled { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the duty finder accept setting is enabled.
    /// </summary>
    public bool ContentsFinderConfirmEnabled { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the duty finder one-time accept setting is enabled.
    /// </summary>
    public bool ContentsFinderOneTimeConfirmEnabled { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to remember the last pane in the inclusion shop.
    /// </summary>
    public bool InclusionShopRememberEnabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the first AtkValue for the inclusion shop category change event.
    /// </summary>
    public uint InclusionShopRememberCategory { get; set; } = 0;

    /// <summary>
    /// Gets or sets the second AtkValue for the inclusion shop subcategory change event.
    /// </summary>
    public uint InclusionShopRememberSubcategory { get; set; } = 0;

    /// <summary>
    /// Loads the configuration.
    /// </summary>
    /// <param name="configDirectory">Configuration directory.</param>
    /// <returns>A configuration.</returns>
    public static YesAlreadyConfiguration Load(DirectoryInfo configDirectory)
    {
        var pluginConfigPath = new FileInfo(Path.Combine(configDirectory.Parent!.FullName, "YesAlready.json"));

        if (!pluginConfigPath.Exists)
            return new YesAlreadyConfiguration();

        var data = File.ReadAllText(pluginConfigPath.FullName);
        var conf = JsonConvert.DeserializeObject<YesAlreadyConfiguration>(data);
        return conf ?? new YesAlreadyConfiguration();
    }

    /// <summary>
    /// Upgrade the configuration from a prior version.
    /// </summary>
    public void Upgrade()
    {
    }

    /// <summary>
    /// Save the plugin configuration to disk.
    /// </summary>
    public void Save() => Service.Interface.SavePluginConfig(this);

    /// <summary>
    /// Get all nodes in the tree.
    /// </summary>
    /// <returns>All the nodes.</returns>
    public IEnumerable<ITextNode> GetAllNodes()
    {
        return new ITextNode[]
        {
            this.RootFolder,
            this.ListRootFolder,
            this.TalkRootFolder,
        }
        .Concat(this.GetAllNodes(this.RootFolder.Children))
        .Concat(this.GetAllNodes(this.ListRootFolder.Children))
        .Concat(this.GetAllNodes(this.TalkRootFolder.Children));
    }

    /// <summary>
    /// Gets all the nodes in this subset of the tree.
    /// </summary>
    /// <param name="nodes">Nodes to search.</param>
    /// <returns>The nodes in the tree.</returns>
    public IEnumerable<ITextNode> GetAllNodes(IEnumerable<ITextNode> nodes)
    {
        foreach (var node in nodes)
        {
            yield return node;
            if (node is TextFolderNode folder)
            {
                var children = this.GetAllNodes(folder.Children);
                foreach (var childNode in children)
                {
                    yield return childNode;
                }
            }
        }
    }

    /// <summary>
    /// Tries to find the parent of a node.
    /// </summary>
    /// <param name="node">Node to check.</param>
    /// <param name="parent">Parent of the node or null.</param>
    /// <returns>A value indicating whether the parent was found.</returns>
    public bool TryFindParent(ITextNode node, out TextFolderNode? parent)
    {
        foreach (var candidate in this.GetAllNodes())
        {
            if (candidate is TextFolderNode folder && folder.Children.Contains(node))
            {
                parent = folder;
                return true;
            }
        }

        parent = null;
        return false;
    }

    /// <summary>
    /// Create a new node with various options.
    /// </summary>
    /// <param name="folder">Folder to place the node in.</param>
    /// <param name="zoneRestricted">Create the node restricted to the current zone.</param>
    /// <param name="createFolder">Create a zone named subfolder.</param>
    /// <param name="selectNo">Select no instead.</param>
    public void CreateTextNode(TextFolderNode folder, bool zoneRestricted, bool createFolder, bool selectNo)
    {
        var newNode = new TextEntryNode() { Enabled = true, Text = Service.Plugin.LastSeenDialogText };
        var chosenFolder = folder;

        if (zoneRestricted || createFolder)
        {
            var currentID = Service.ClientState.TerritoryType;
            if (!Service.Plugin.TerritoryNames.TryGetValue(currentID, out var zoneName))
                return;

            newNode.ZoneRestricted = true;
            newNode.ZoneText = zoneName;
        }

        if (createFolder)
        {
            var zoneName = newNode.ZoneText;

            chosenFolder = folder.Children.OfType<TextFolderNode>().FirstOrDefault(node => node.Name == zoneName);
            if (chosenFolder == default)
            {
                chosenFolder = new TextFolderNode { Name = zoneName };
                folder.Children.Add(chosenFolder);
            }
        }

        if (selectNo)
        {
            newNode.IsYes = false;
        }

        chosenFolder.Children.Add(newNode);
    }
}
