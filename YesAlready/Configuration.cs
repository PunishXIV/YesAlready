using System.Collections.Generic;
using System.IO;
using System.Linq;

using Dalamud.Configuration;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Text;
using Newtonsoft.Json;

namespace YesAlready;

public partial class Configuration() : IPluginConfiguration
{
    public int Version { get; set; } = 1;
    public bool Enabled { get; set; } = true;
    public XivChatType MessageChannel { get; set; } = Svc.PluginInterface.GeneralChatType;

    public VirtualKey ForcedYesKey { get; set; } = VirtualKey.NO_KEY;
    public VirtualKey ForcedTalkKey { get; set; } = VirtualKey.NO_KEY;
    public VirtualKey DisableKey { get; set; } = VirtualKey.NO_KEY;
    public bool SeparateForcedKeys { get; set; } = false;
    public TextFolderNode RootFolder { get; private set; } = new TextFolderNode { Name = "/" };
    public TextFolderNode OkRootFolder { get; private set; } = new TextFolderNode { Name = "/" };
    public TextFolderNode ListRootFolder { get; private set; } = new TextFolderNode { Name = "/" };
    public TextFolderNode TalkRootFolder { get; private set; } = new TextFolderNode { Name = "/" };
    public TextFolderNode NumericsRootFolder { get; private set; } = new TextFolderNode { Name = "/" };
    public bool DesynthDialogEnabled { get; set; } = false;
    public bool DesynthBulkDialogEnabled { get; set; } = false;
    public bool MaterializeDialogEnabled { get; set; } = false;
    public bool MaterialAttachDialogEnabled { get; set; } = false;
    public bool OnlyMeldWhenGuaranteed { get; set; } = true;
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
    public bool GuildLeveDifficultyConfirm { get; set; } = false;
    public bool FallGuysRegisterConfirm { get; set; } = false;
    public bool FallGuysExitConfirm { get; set; } = false;
    public bool RetainerTransferListConfirm { get; set; } = false;
    public bool RetainerTransferProgressConfirm { get; set; } = false;
    public bool DesynthesisResults { get; set; } = false;
    public bool AetherialReductionResults { get; set; } = false;
    public bool FashionCheckQuit { get; set; } = false;
    public bool LordOfVerminionQuit { get; set; } = false;
    public bool ChocoboRacingQuit { get; set; } = false;
    public bool PartyFinderJoinConfirm { get; set; } = false;
    public bool GimmickYesNo { get; set; } = false;
    public bool AutoCollectable { get; set; } = false;
    public bool LotteryWeeklyInput { get; set; } = false;
    public bool TradeMultiple { get; set; } = false;
    public TradeMultipleMode TransmuteMode { get; set; } = TradeMultipleMode.AllSame;
    public bool KupoOfFortune { get; set; } = false;
    public bool CustomDeliveries { get; set; } = false;
    public bool MKSRecordQuit { get; set; } = false;
    public bool FrontlineRecordQuit { get; set; } = false;
    public bool DataCentreTravelConfirmEnabled { get; set; } = false;

    public List<CustomBother> CustomBothers { get; set; } = [];

    public class CustomBother
    {
        public string Addon { get; set; }
        public bool UpdateState { get; set; } = true;
        public object[] CallbackParams { get; set; }
    }

    public enum TradeMultipleMode
    {
        AllSame = 0,
        AllDifferent = 1,
    }

    public static Configuration Load(DirectoryInfo configDirectory)
    {
        var pluginConfigPath = new FileInfo(Path.Combine(configDirectory.Parent!.FullName, "YesAlready.json"));

        if (!pluginConfigPath.Exists)
            return new Configuration();

        var data = File.ReadAllText(pluginConfigPath.FullName);
        var conf = JsonConvert.DeserializeObject<Configuration>(data);
        return conf ?? new Configuration();
    }

    public void Save() => Svc.PluginInterface.SavePluginConfig(this);

    public IEnumerable<ITextNode> GetAllNodes()
    {
        return new ITextNode[]
        {
            RootFolder,
            OkRootFolder,
            ListRootFolder,
            TalkRootFolder,
            NumericsRootFolder,
        }
        .Concat(GetAllNodes(RootFolder.Children))
        .Concat(GetAllNodes(OkRootFolder.Children))
        .Concat(GetAllNodes(ListRootFolder.Children))
        .Concat(GetAllNodes(TalkRootFolder.Children))
        .Concat(GetAllNodes(NumericsRootFolder.Children));
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

    public static void CreateOkNode(TextFolderNode folder, bool createFolder)
    {
        var newNode = new OkEntryNode() { Enabled = true, Text = P.LastSeenOkText };
        var chosenFolder = folder;

        if (createFolder)
        {
            if (chosenFolder == default)
            {
                chosenFolder = new TextFolderNode { Name = chosenFolder.Name };
                folder.Children.Add(chosenFolder);
            }
        }

        chosenFolder.Children.Add(newNode);
    }

    public static void CreateNumericsNode(TextFolderNode folder, bool createFolder)
    {
        var newNode = new NumericsEntryNode() { Enabled = true, Text = P.LastSeenNumericsText };
        var chosenFolder = folder;

        if (createFolder)
        {
            if (chosenFolder == default)
            {
                chosenFolder = new TextFolderNode { Name = chosenFolder.Name };
                folder.Children.Add(chosenFolder);
            }
        }

        chosenFolder.Children.Add(newNode);
    }
}
