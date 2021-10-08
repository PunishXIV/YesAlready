using System.Collections.Generic;
using System.IO;
using System.Linq;

using Dalamud.Configuration;
using Newtonsoft.Json;

namespace YesAlready
{
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
        /// Gets the root folder.
        /// </summary>
        public TextFolderNode RootFolder { get; private set; } = new TextFolderNode { Name = "/" };

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
        /// Loads the configuration.
        /// </summary>
        /// <param name="configDirectory">Configuration directory.</param>
        /// <returns>A configuration.</returns>
        public static YesAlreadyConfiguration Load(DirectoryInfo configDirectory)
        {
            var pluginConfigPath = new FileInfo(Path.Combine(configDirectory.Parent!.FullName, $"YesAlready.json"));

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
            return new ITextNode[] { this.RootFolder }.Concat(this.GetAllNodes(this.RootFolder.Children));
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
    }
}
