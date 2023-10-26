using System.Collections.Generic;
using System.IO;
using System.Linq;

using Dalamud.Configuration;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Plugin;
using ECommons.DalamudServices;
using Newtonsoft.Json;

namespace YesAlready;

public partial class Configuration() : IPluginConfiguration
{
    public int Version { get; set; } = 1;
    public bool Enabled { get; set; } = true;

    public VirtualKey ForcedYesKey { get; set; } = VirtualKey.NO_KEY;
    public VirtualKey DisableKey { get; set; } = VirtualKey.NO_KEY;
    public TextFolderNode RootFolder { get; private set; } = new TextFolderNode { Name = "/" };
    public TextFolderNode ListRootFolder { get; private set; } = new TextFolderNode { Name = "/" };
    public TextFolderNode TalkRootFolder { get; private set; } = new TextFolderNode { Name = "/" };
    public bool DesynthDialogEnabled { get; set; } = false;
    public bool DesynthBulkDialogEnabled { get; set; } = false;
    public bool MaterializeDialogEnabled { get; set; } = false;
    public bool MateriaRetrieveDialogEnabled { get; set; } = false;
    public bool ItemInspectionResultEnabled { get; set; } = false;
    public int ItemInspectionResultRateLimiter { get; set; } = 0;
    public bool RetainerTaskAskEnabled { get; set; } = false;
    public bool RetainerTaskResultEnabled { get; set; } = false;
    public bool GrandCompanySupplyReward { get; set; } = false;
    public bool ShopCardDialog { get; set; } = false;
    public bool ShopExchangeItemDialogEnabled { get; set; } = false;
    public bool JournalResultCompleteEnabled { get; set; } = false;
    public bool ContentsFinderConfirmEnabled { get; set; } = false;
    public bool ContentsFinderOneTimeConfirmEnabled { get; set; } = false;
    public bool InclusionShopRememberEnabled { get; set; } = false;
    public uint InclusionShopRememberCategory { get; set; } = 0;
    public uint InclusionShopRememberSubcategory { get; set; } = 0;
    public bool GuildLeveDifficultyConfirm {  get; set; } = false;

    public bool DTRSupport { get; set; } = true;

    public static Configuration Load(DirectoryInfo configDirectory)
    {
        var pluginConfigPath = new FileInfo(Path.Combine(configDirectory.Parent!.FullName, "YesAlready.json"));

        if (!pluginConfigPath.Exists)
            return new Configuration();

        var data = File.ReadAllText(pluginConfigPath.FullName);
        var conf = JsonConvert.DeserializeObject<Configuration>(data);
        return conf ?? new Configuration();
    }

    public void Save() => pi.SavePluginConfig(this);

    public static void Initialize(DalamudPluginInterface pluginInterface) => pi = pluginInterface;

    public IEnumerable<ITextNode> GetAllNodes()
    {
        return new ITextNode[]
        {
            RootFolder,
            ListRootFolder,
            TalkRootFolder,
        }
        .Concat(GetAllNodes(RootFolder.Children))
        .Concat(GetAllNodes(ListRootFolder.Children))
        .Concat(GetAllNodes(TalkRootFolder.Children));
    }

    public IEnumerable<ITextNode> GetAllNodes(IEnumerable<ITextNode> nodes)
    {
        foreach (var node in nodes)
        {
            yield return node;
            if (node is TextFolderNode folder)
            {
                var children = GetAllNodes(folder.Children);
                foreach (var childNode in children)
                {
                    yield return childNode;
                }
            }
        }
    }

    public bool TryFindParent(ITextNode node, out TextFolderNode? parent)
    {
        foreach (var candidate in GetAllNodes())
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

    public static void CreateTextNode(TextFolderNode folder, bool zoneRestricted, bool createFolder, bool selectNo)
    {
        var newNode = new TextEntryNode() { Enabled = true, Text = P.LastSeenDialogText };
        var chosenFolder = folder;

        if (zoneRestricted || createFolder)
        {
            var currentID = Svc.ClientState.TerritoryType;
            if (!P.TerritoryNames.TryGetValue(currentID, out var zoneName))
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
