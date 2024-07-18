using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Text;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ECommons;
using ECommons.Reflection;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
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

    private readonly string[] hotkeyChoices =
    [
        "None",
        "Control",
        "Alt",
        "Shift",
    ];

    private readonly VirtualKey[] hotkeyValues =
    [
        VirtualKey.NO_KEY,
        VirtualKey.CONTROL,
        VirtualKey.MENU,
        VirtualKey.SHIFT,
    ];

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
            DisplayBotherOptions();
            DisplayMiscOptions();
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

    private void DisplayBotherOptions()
    {
        using var tab = ImRaii.TabItem("Bothers");
        if (!tab) return;
        using var idScope = ImRaii.PushId($"BothersOptions");

        #region Disable hotkey

        if (!hotkeyValues.Contains(P.Config.DisableKey))
        {
            P.Config.DisableKey = VirtualKey.NO_KEY;
            P.Config.Save();
        }

        var disableHotkeyIndex = Array.IndexOf(hotkeyValues, P.Config.DisableKey);

        ImGui.SetNextItemWidth(85);
        if (ImGui.Combo("Disable Hotkey", ref disableHotkeyIndex, hotkeyChoices, hotkeyChoices.Length))
        {
            P.Config.DisableKey = hotkeyValues[disableHotkeyIndex];
            P.Config.Save();
        }

        ImGuiEx.IndentedTextColored($"While this key is held, the plugin is disabled.");

        #endregion
        #region Forced Yes hotkey

        if (!hotkeyValues.Contains(P.Config.ForcedYesKey))
        {
            P.Config.ForcedYesKey = VirtualKey.NO_KEY;
            P.Config.Save();
        }

        var forcedYesHotkeyIndex = Array.IndexOf(hotkeyValues, P.Config.ForcedYesKey);

        ImGui.SetNextItemWidth(85);
        if (ImGui.Combo("Forced Yes Hotkey", ref forcedYesHotkeyIndex, hotkeyChoices, hotkeyChoices.Length))
        {
            P.Config.ForcedYesKey = hotkeyValues[forcedYesHotkeyIndex];
            P.Config.Save();
        }

        ImGui.SameLine();
        var separateForcedKeys = P.Config.SeparateForcedKeys;
        if (ImGui.Checkbox("Separate Yes/Talk", ref separateForcedKeys))
        {
            P.Config.SeparateForcedKeys = separateForcedKeys;
            P.Config.Save();
        }

        if (P.Config.SeparateForcedKeys)
        {
            var forcedTalkHotkeyIndex = Array.IndexOf(hotkeyValues, P.Config.ForcedTalkKey);
            ImGui.SetNextItemWidth(85);
            if (ImGui.Combo("Forced Talk Hotkey", ref forcedTalkHotkeyIndex, hotkeyChoices, hotkeyChoices.Length))
            {
                P.Config.ForcedTalkKey = hotkeyValues[forcedTalkHotkeyIndex];
                P.Config.Save();
            }
        }

        ImGuiEx.IndentedTextColored($"While this key is held, any Yes/No prompt will always default to yes and all talk dialogue will be skipped. Be careful.");

        #endregion
        #region SalvageDialog

        var desynthDialog = P.Config.DesynthDialogEnabled;
        if (ImGui.Checkbox("SalvageDialog", ref desynthDialog))
        {
            P.Config.DesynthDialogEnabled = desynthDialog;
            P.Config.Save();
        }

        ImGuiEx.IndentedTextColored("Remove the Desynthesis menu confirmation.");

        #endregion
        #region SalvageDialog (Bulk)
        using (ImRaii.Disabled())
        {
            var desynthBulkDialog = P.Config.DesynthBulkDialogEnabled;
            if (ImGui.Checkbox("SalvageDialog (Bulk)", ref desynthBulkDialog))
            {
                P.Config.DesynthBulkDialogEnabled = desynthBulkDialog;
                P.Config.Save();
            }

            ImGuiEx.IndentedTextColored("Check the bulk desynthesis button when using the SalvageDialog feature.\nThis feature has been incorporated into vanilla. The checkbox state is now remembered.");
        }
        #endregion
        #region SalvageResult

        var desynthResultsDialog = P.Config.DesynthesisResults;
        if (ImGui.Checkbox("SalvageResults", ref desynthResultsDialog))
        {
            P.Config.DesynthesisResults = desynthResultsDialog;
            P.Config.Save();
        }

        ImGuiEx.IndentedTextColored("Automatically closes the SalvageResults window when done desynthing.");

        #endregion
        #region PurifyResult

        var purifyResultsDialog = P.Config.AetherialReductionResults;
        if (ImGui.Checkbox("PurifyResult", ref purifyResultsDialog))
        {
            P.Config.AetherialReductionResults = purifyResultsDialog;
            P.Config.Save();
        }

        ImGuiEx.IndentedTextColored("Automatically closes the PurifyResult window when done reducing.");

        #endregion
        #region MateriaAttachDialog

        var meld = P.Config.MaterialAttachDialogEnabled;
        if (ImGui.Checkbox("MateriaAttachDialog", ref meld))
        {
            P.Config.MaterialAttachDialogEnabled = meld;
            P.Config.Save();
        }

        ImGuiEx.IndentedTextColored("Remove the materia melding confirmation menu.");

        if (P.Config.MaterialAttachDialogEnabled)
        {
            ImGui.Indent();
            var onlySuccess = P.Config.OnlyMeldWhenGuaranteed;
            if (ImGui.Checkbox("Only function when the success rate is guaranteed (100%)", ref onlySuccess))
            {
                P.Config.OnlyMeldWhenGuaranteed = onlySuccess;
                P.Config.Save();
            }
            ImGui.Unindent();
        }

        #endregion
        #region MaterializeDialog

        var materialize = P.Config.MaterializeDialogEnabled;
        if (ImGui.Checkbox("MaterializeDialog", ref materialize))
        {
            P.Config.MaterializeDialogEnabled = materialize;
            P.Config.Save();
        }

        ImGuiEx.IndentedTextColored("Remove the create new (extract) materia confirmation.");

        #endregion
        #region MateriaRetrieveDialog

        var materiaRetrieve = P.Config.MateriaRetrieveDialogEnabled;
        if (ImGui.Checkbox("MateriaRetrieveDialog", ref materiaRetrieve))
        {
            P.Config.MateriaRetrieveDialogEnabled = materiaRetrieve;
            P.Config.Save();
        }

        ImGuiEx.IndentedTextColored("Remove the retrieve materia confirmation.");

        #endregion
        #region ItemInspectionResult

        var itemInspection = P.Config.ItemInspectionResultEnabled;
        if (ImGui.Checkbox("ItemInspectionResult", ref itemInspection))
        {
            P.Config.ItemInspectionResultEnabled = itemInspection;
            P.Config.Save();
        }

        ImGuiEx.IndentedTextColored("Eureka/Bozja lockboxes, forgotten fragments, and more.\nWarning: this does not check if you are maxed on items.");

        ImGuiEx.IndentedTextColored("Rate limiter (pause after N items)");
        ImGui.SameLine();

        ImGui.PushItemWidth(100f * ImGuiHelpers.GlobalScale);
        var itemInspectionResultLimiter = P.Config.ItemInspectionResultRateLimiter;
        if (ImGui.InputInt("###itemInspectionResultRateLimiter", ref itemInspectionResultLimiter))
        {
            if (itemInspectionResultLimiter < 0)
            {
                itemInspectionResultLimiter = 0;
            }
            else
            {
                P.Config.ItemInspectionResultRateLimiter = itemInspectionResultLimiter;
                P.Config.Save();
            }
        }

        #endregion
        #region RetainerTaskAsk

        var retainerTaskAsk = P.Config.RetainerTaskAskEnabled;
        if (ImGui.Checkbox("RetainerTaskAsk", ref retainerTaskAsk))
        {
            P.Config.RetainerTaskAskEnabled = retainerTaskAsk;
            P.Config.Save();
        }

        ImGuiEx.IndentedTextColored("Skip the confirmation in the final dialog before sending out a retainer.");

        #endregion
        #region RetainerTaskResult

        var retainerTaskResult = P.Config.RetainerTaskResultEnabled;
        if (ImGui.Checkbox("RetainerTaskResult", ref retainerTaskResult))
        {
            P.Config.RetainerTaskResultEnabled = retainerTaskResult;
            P.Config.Save();
        }

        ImGuiEx.IndentedTextColored("Automatically send a retainer on the same venture as before when receiving an item.");

        #endregion
        #region RetainerTransferList

        var retainerListDialog = P.Config.RetainerTransferListConfirm;
        if (ImGui.Checkbox("RetainerItemTransferList", ref retainerListDialog))
        {
            P.Config.RetainerTransferListConfirm = retainerListDialog;
            P.Config.Save();
        }

        ImGuiEx.IndentedTextColored("Skip the confirmation in the RetainerItemTransferList window to entrust all items to the retainer.");

        #endregion
        #region RetainerTransferProgress

        var retainerProgressDialog = P.Config.RetainerTransferProgressConfirm;
        if (ImGui.Checkbox("RetainerItemTransferProgress", ref retainerProgressDialog))
        {
            P.Config.RetainerTransferProgressConfirm = retainerProgressDialog;
            P.Config.Save();
        }

        ImGuiEx.IndentedTextColored("Automatically closes the RetainerItemTransferProgress window when finished entrusting items.");

        #endregion
        #region GrandCompanySupplyReward

        var grandCompanySupplyReward = P.Config.GrandCompanySupplyReward;
        if (ImGui.Checkbox("GrandCompanySupplyReward", ref grandCompanySupplyReward))
        {
            P.Config.GrandCompanySupplyReward = grandCompanySupplyReward;
            P.Config.Save();
        }

        ImGuiEx.IndentedTextColored("Skip the confirmation when submitting Grand Company expert delivery items.");

        #endregion
        #region ShopCardDialog

        var shopCard = P.Config.ShopCardDialog;
        if (ImGui.Checkbox("ShopCardDialog", ref shopCard))
        {
            P.Config.ShopCardDialog = shopCard;
            P.Config.Save();
        }

        ImGuiEx.IndentedTextColored("Automatically confirm selling Triple Triad cards in the saucer.");

        #endregion
        #region JournalResultComplete

        var journalResultComplete = P.Config.JournalResultCompleteEnabled;
        if (ImGui.Checkbox("JournalResultComplete", ref journalResultComplete))
        {
            P.Config.JournalResultCompleteEnabled = journalResultComplete;
            P.Config.Save();
        }

        ImGuiEx.IndentedTextColored("Automatically confirm quest reward acceptance when there is nothing to choose.");

        #endregion
        #region ContentFinderConfirm

        var contentsFinderConfirm = P.Config.ContentsFinderConfirmEnabled;
        if (ImGui.Checkbox("ContentsFinderConfirm", ref contentsFinderConfirm))
        {
            P.Config.ContentsFinderConfirmEnabled = contentsFinderConfirm;

            if (!contentsFinderConfirm)
                P.Config.ContentsFinderOneTimeConfirmEnabled = false;

            P.Config.Save();
        }

        ImGuiEx.IndentedTextColored("Automatically commence duties when ready.");

        #endregion
        #region ContentFinderOneTimeConfirm

        var contentsFinderOneTimeConfirm = P.Config.ContentsFinderOneTimeConfirmEnabled;
        if (ImGui.Checkbox("ContentsFinderOneTimeConfirm", ref contentsFinderOneTimeConfirm))
        {
            P.Config.ContentsFinderOneTimeConfirmEnabled = contentsFinderOneTimeConfirm;

            if (contentsFinderOneTimeConfirm)
                P.Config.ContentsFinderConfirmEnabled = true;

            P.Config.Save();
        }

        ImGuiEx.IndentedTextColored("Automatically commence duties when ready, but only once.\nRequires Contents Finder Confirm, and disables both after activation.");

        #endregion
        #region InclusionShop

        var inclusionShopRemember = P.Config.InclusionShopRememberEnabled;
        if (ImGui.Checkbox("InclusionShopRemember", ref inclusionShopRemember))
        {
            P.Config.InclusionShopRememberEnabled = inclusionShopRemember;
            P.Config.Save();
        }

        ImGuiEx.IndentedTextColored("Remember the last panel visited on the scrip exchange window.");

        #endregion
        #region GuildLeveDifficulty

        var guildLeveDifficulty = P.Config.GuildLeveDifficultyConfirm;
        if (ImGui.Checkbox("GuildLeveDifficulty", ref guildLeveDifficulty))
        {
            P.Config.GuildLeveDifficultyConfirm = guildLeveDifficulty;
            P.Config.Save();
        }

        ImGuiEx.IndentedTextColored("Automatically confirms guild leves upon initiation at the highest difficulty.");

        #endregion
        #region ShopExchangeItemDialog

        var shopItemExchange = P.Config.ShopExchangeItemDialogEnabled;
        if (ImGui.Checkbox("ShopExchangeItemDialog", ref shopItemExchange))
        {
            P.Config.ShopExchangeItemDialogEnabled = shopItemExchange;
            P.Config.Save();
        }

        ImGuiEx.IndentedTextColored("Automatically exchange items/currencies in various shops (e.g. scrip vendors).");

        #endregion
        #region FallGuysRegisterConfirm

        var fgsEnter = P.Config.FallGuysRegisterConfirm;
        if (ImGui.Checkbox("FGSEnterDialog", ref fgsEnter))
        {
            P.Config.FallGuysRegisterConfirm = fgsEnter;
            P.Config.Save();
        }

        ImGuiEx.IndentedTextColored("Automatically register for Blunderville when speaking with the Blunderville Registrar.");

        #endregion
        #region FallGuysExitConfirm

        var fgsExit = P.Config.FallGuysExitConfirm;
        if (ImGui.Checkbox("FGSExitDialog", ref fgsExit))
        {
            P.Config.FallGuysExitConfirm = fgsExit;
            P.Config.Save();
        }

        ImGuiEx.IndentedTextColored("Automatically confirm the exit prompt when leaving Blunderville.");

        #endregion
        #region FashionCheckQuit

        var fashionQuit = P.Config.FashionCheckQuit;
        if (ImGui.Checkbox("FashionCheck", ref fashionQuit))
        {
            P.Config.FashionCheckQuit = fashionQuit;
            P.Config.Save();
        }

        ImGuiEx.IndentedTextColored("Automatically confirm the Fashion Reports results.");

        #endregion
        #region ChocoboRacingQuit

        var chocoboQuit = P.Config.ChocoboRacingQuit;
        if (ImGui.Checkbox("RaceChocoboResult", ref chocoboQuit))
        {
            P.Config.ChocoboRacingQuit = chocoboQuit;
            P.Config.Save();
        }

        ImGuiEx.IndentedTextColored("Automatically quit Chocobo Racing when the resuls menu appears.");

        #endregion
        #region LordOfVerminionQuit

        var lovQuit = P.Config.LordOfVerminionQuit;
        if (ImGui.Checkbox("LovmResult", ref lovQuit))
        {
            P.Config.LordOfVerminionQuit = lovQuit;
            P.Config.Save();
        }

        ImGuiEx.IndentedTextColored("Automatically quit Lord of Verminion when the results menu appears.");

        #endregion   
        #region LotteryWeeklyInput

        var lotto = P.Config.LotteryWeeklyInput;
        if (ImGui.Checkbox("LotteryWeeklyInput", ref lotto))
        {
            P.Config.LotteryWeeklyInput = lotto;
            P.Config.Save();
        }

        ImGuiEx.IndentedTextColored("Automatically purchase a Jumbo Cactpot ticket with a random number.");

        #endregion
        //#region TradeMultiple

        //var transmute = P.Config.TradeMultiple;
        //if (ImGui.Checkbox("TradeMultiple", ref transmute))
        //{
        //    P.Config.TradeMultiple = transmute;
        //    P.Config.Save();
        //}

        //ImGuiEx.IndentedTextColored("Automatically turn in materia to be transmuted. Will transmute any and all materia in inventory.");

        //#endregion
        #region HWDLottery

        var kupo = P.Config.KupoOfFortune;
        if (ImGui.Checkbox("HWDLottery", ref kupo))
        {
            P.Config.KupoOfFortune = kupo;
            P.Config.Save();
        }

        ImGuiEx.IndentedTextColored("Automatically select a kupo of fortune reward. This will instantly complete a single kupo ticket but is unable to continue to the next automatically.");

        #endregion
        #region SatisfactionSupply

        var deliveries = P.Config.CustomDeliveries;
        if (ImGui.Checkbox("SatisfactionSupply", ref deliveries))
        {
            P.Config.CustomDeliveries = deliveries;
            P.Config.Save();
        }

        ImGuiEx.IndentedTextColored("Automatically turn in any available collectables for Custom Deliveries.");

        #endregion
        #region MKSRecordQuit

        var ccquit = P.Config.MKSRecordQuit;
        if (ImGui.Checkbox("MKSRecord", ref ccquit))
        {
            P.Config.MKSRecordQuit = ccquit;
            P.Config.Save();
        }

        ImGuiEx.IndentedTextColored("Automatically leave the Crystalline Conflict match when the results appear.");

        #endregion
        #region DataCentreTravelConfirm

        var dkt = P.Config.DataCentreTravelConfirmEnabled;
        if (ImGui.Checkbox("DataCentreTravelConfirm", ref dkt))
        {
            P.Config.DataCentreTravelConfirmEnabled = dkt;
            P.Config.Save();
        }

        ImGuiEx.IndentedTextColored("Automatically accept the Data Center travel confirmation.");

        #endregion
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
        {
            DisplayTextNode(node, root);
        }
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
            {
                DisplayTextNode(childNode, root);
            }

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
