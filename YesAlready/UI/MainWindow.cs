using System;
using System.Numerics;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Utility;
using Dalamud.Interface;
using System.Text;
using Dalamud.Interface.Windowing;
using ECommons.ImGuiMethods;
using ImGuiNET;
using ECommons.DalamudServices;
using ClickLib.Exceptions;
using System.Linq;
using Dalamud.Interface.Colors;
using ECommons;
using ECommons.Reflection;
using System.Collections.Generic;

namespace YesAlready.UI;

internal class MainWindow : Window
{
    public MainWindow() : base($"{Name} {P.GetType().Assembly.GetName().Version}###{Name}")
    {
        Size = new Vector2(525, 600);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    private readonly Vector4 shadedColor = new(0.68f, 0.68f, 0.68f, 1.0f);

    private readonly string[] hotkeyChoices = new[]
    {
        "None",
        "Control",
        "Alt",
        "Shift",
    };

    private readonly VirtualKey[] hotkeyValues = new[]
    {
        VirtualKey.NO_KEY,
        VirtualKey.CONTROL,
        VirtualKey.MENU,
        VirtualKey.SHIFT,
    };

    private static readonly ComparisonType[] ComparisonTypes =
    [
        ComparisonType.LessThan,
        ComparisonType.LessThanOrEqual,
        ComparisonType.GreaterThan,
        ComparisonType.GreaterThanOrEqual,
    ];

    private ITextNode? draggedNode = null;
    private string debugClickName = string.Empty;

    private static TextFolderNode RootFolder => P.Config.RootFolder;

    private static TextFolderNode OkRootFolder => P.Config.OkRootFolder;

    private static TextFolderNode ListRootFolder => P.Config.ListRootFolder;

    private static TextFolderNode TalkRootFolder => P.Config.TalkRootFolder;

    public override void PreDraw() => ImGui.PushStyleColor(ImGuiCol.ResizeGrip, 0);

    public override void PostDraw() => ImGui.PopStyleColor();

    public override void Draw()
    {
#if DEBUG
        UiBuilder_TestButton();
        if (ImGui.Button("Enable Features")) EnableFeatures(true);
        ImGui.SameLine();
        if (ImGui.Button("Disable Features")) EnableFeatures(false);
#endif

        if (P.BlockListHandler.Locked)
        {
            ImGuiEx.TextWrapped(ImGuiColors.DalamudRed, $"Yes Already function is paused because following plugins have requested it: {P.BlockListHandler.BlockList.Print()}");
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

        if (ImGui.BeginTabBar("Settings"))
        {
            DisplayTextOptions();
            DisplayOkOptions();
            DisplayListOptions();
            DisplayTalkOptions();
            DisplayBotherOptions();
            DisplayMiscOptions();

            //if (ImGui.BeginTabItem("About"))
            //{
            //    AboutTab.Draw("YesAlready");
            //    ImGui.EndTabItem();
            //}

            ImGui.EndTabBar();
        }
    }

    private static void IndentedTextColored(Vector4 color, string text)
    {
        var indent = 27f * ImGuiHelpers.GlobalScale;
        ImGui.Indent(indent);
        ImGui.PushStyleColor(ImGuiCol.Text, color);
        ImGui.TextWrapped(text);
        ImGui.PopStyleColor();
        ImGui.Unindent(indent);
    }

    #region Testing

    private void UiBuilder_TestButton()
    {
        ImGui.InputText("ClickName", ref debugClickName, 100);
        ImGui.SameLine();
        if (ImGuiEx.IconButton(FontAwesomeIcon.Check, "Submit"))
        {
            try
            {
                debugClickName ??= string.Empty;
                ClickLib.Click.SendClick(debugClickName.Trim());
                Svc.Log.Info($"Clicked {debugClickName} successfully.");
            }
            catch (ClickNotFoundError ex)
            {
                Svc.Log.Error(ex.Message);
            }
            catch (InvalidClickException ex)
            {
                Svc.Log.Error(ex.Message);
            }
            catch (Exception ex)
            {
                Svc.Log.Error(ex.Message);
            }
        }
    }

    #endregion

    // ====================================================================================================

    private void DisplayMiscOptions()
    {
        if (!ImGui.BeginTabItem("Settings"))
            return;

        ImGui.PushID("Server info bar");

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
            IndentedTextColored(shadedColor, $"Display the status of the {Name} in the Server Info Bar (DTR Bar). Clicking toggles the plugin.");
        }
        catch (Exception e)
        {
            ImGuiEx.TextWrapped(ImGuiColors.DalamudRed, $"{e}");
        }

        ImGui.PopID();

        ImGui.EndTabItem();
    }

    private void DisplayTextOptions()
    {
        if (!ImGui.BeginTabItem("YesNo"))
            return;

        ImGui.PushID("TextOptions");

        DisplayTextButtons();
        DisplayTextNodes();

        ImGui.PopID();

        ImGui.EndTabItem();
    }

    private void DisplayOkOptions()
    {
        if (!ImGui.BeginTabItem("Ok"))
            return;

        ImGui.PushID("OkOptions");

        DisplayOkButtons();
        DisplayOkNodes();

        ImGui.PopID();

        ImGui.EndTabItem();
    }

    private void DisplayListOptions()
    {
        if (!ImGui.BeginTabItem("Lists"))
            return;

        ImGui.PushID("ListOptions");

        DisplayListButtons();
        DisplayListNodes();

        ImGui.PopID();

        ImGui.EndTabItem();
    }

    private void DisplayTalkOptions()
    {
        if (!ImGui.BeginTabItem("Talk"))
            return;

        ImGui.PushID("TalkOptions");

        DisplayTalkButtons();
        DisplayTalkNodes();

        ImGui.PopID();

        ImGui.EndTabItem();
    }

    private void DisplayBotherOptions()
    {
        if (!ImGui.BeginTabItem("Bothers"))
            return;

        ImGui.PushID("BotherOptions");

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

        IndentedTextColored(shadedColor, $"While this key is held, the plugin is disabled.");

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

        IndentedTextColored(shadedColor, $"While this key is held, any Yes/No prompt will always default to yes. Be careful.");

        #endregion
        #region SalvageDialog

        var desynthDialog = P.Config.DesynthDialogEnabled;
        if (ImGui.Checkbox("SalvageDialog", ref desynthDialog))
        {
            P.Config.DesynthDialogEnabled = desynthDialog;
            P.Config.Save();
        }

        IndentedTextColored(shadedColor, "Remove the Desynthesis menu confirmation.");

        #endregion
        #region SalvageDialog (Bulk)

        var desynthBulkDialog = P.Config.DesynthBulkDialogEnabled;
        if (ImGui.Checkbox("SalvageDialog (Bulk)", ref desynthBulkDialog))
        {
            P.Config.DesynthBulkDialogEnabled = desynthBulkDialog;
            P.Config.Save();
        }

        IndentedTextColored(shadedColor, "Check the bulk desynthesis button when using the SalvageDialog feature.");

        #endregion
        #region SalvageResult

        var desynthResultsDialog = P.Config.DesynthesisResults;
        if (ImGui.Checkbox("SalvageResults", ref desynthResultsDialog))
        {
            P.Config.DesynthesisResults = desynthResultsDialog;
            P.Config.Save();
        }

        IndentedTextColored(shadedColor, "Automatically closes the SalvageResults window when done desynthing.");

        #endregion
        #region PurifyResult

        var purifyResultsDialog = P.Config.AetherialReductionResults;
        if (ImGui.Checkbox("PurifyResult", ref purifyResultsDialog))
        {
            P.Config.AetherialReductionResults = purifyResultsDialog;
            P.Config.Save();
        }

        IndentedTextColored(shadedColor, "Automatically closes the PurifyResult window when done reducing.");

        #endregion
        #region MaterializeDialog

        var materialize = P.Config.MaterializeDialogEnabled;
        if (ImGui.Checkbox("MaterializeDialog", ref materialize))
        {
            P.Config.MaterializeDialogEnabled = materialize;
            P.Config.Save();
        }

        IndentedTextColored(shadedColor, "Remove the create new (extract) materia confirmation.");

        #endregion
        #region MateriaRetrieveDialog

        var materiaRetrieve = P.Config.MateriaRetrieveDialogEnabled;
        if (ImGui.Checkbox("MateriaRetrieveDialog", ref materiaRetrieve))
        {
            P.Config.MateriaRetrieveDialogEnabled = materiaRetrieve;
            P.Config.Save();
        }

        IndentedTextColored(shadedColor, "Remove the retrieve materia confirmation.");

        #endregion
        #region ItemInspectionResult

        var itemInspection = P.Config.ItemInspectionResultEnabled;
        if (ImGui.Checkbox("ItemInspectionResult", ref itemInspection))
        {
            P.Config.ItemInspectionResultEnabled = itemInspection;
            P.Config.Save();
        }

        IndentedTextColored(shadedColor, "Eureka/Bozja lockboxes, forgotten fragments, and more.\nWarning: this does not check if you are maxed on items.");

        IndentedTextColored(shadedColor, "Rate limiter (pause after N items)");
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

        IndentedTextColored(shadedColor, "Skip the confirmation in the final dialog before sending out a retainer.");

        #endregion
        #region RetainerTaskResult

        var retainerTaskResult = P.Config.RetainerTaskResultEnabled;
        if (ImGui.Checkbox("RetainerTaskResult", ref retainerTaskResult))
        {
            P.Config.RetainerTaskResultEnabled = retainerTaskResult;
            P.Config.Save();
        }

        IndentedTextColored(shadedColor, "Automatically send a retainer on the same venture as before when receiving an item.");

        #endregion
        #region RetainerTransferList

        var retainerListDialog = P.Config.RetainerTransferListConfirm;
        if (ImGui.Checkbox("RetainerItemTransferList", ref retainerListDialog))
        {
            P.Config.RetainerTransferListConfirm = retainerListDialog;
            P.Config.Save();
        }

        IndentedTextColored(shadedColor, "Skip the confirmation in the RetainerItemTransferList window to entrust all items to the retainer.");

        #endregion
        #region RetainerTransferProgress



        var retainerProgressDialog = P.Config.RetainerTransferProgressConfirm;
        if (ImGui.Checkbox("RetainerItemTransferProgress", ref retainerProgressDialog))
        {
            P.Config.RetainerTransferProgressConfirm = retainerProgressDialog;
            P.Config.Save();
        }

        IndentedTextColored(shadedColor, "Automatically closes the RetainerItemTransferProgress window when finished entrusting items.");

        #endregion
        #region GrandCompanySupplyReward

        var grandCompanySupplyReward = P.Config.GrandCompanySupplyReward;
        if (ImGui.Checkbox("GrandCompanySupplyReward", ref grandCompanySupplyReward))
        {
            P.Config.GrandCompanySupplyReward = grandCompanySupplyReward;
            P.Config.Save();
        }

        IndentedTextColored(shadedColor, "Skip the confirmation when submitting Grand Company expert delivery items.");

        #endregion
        #region ShopCardDialog

        var shopCard = P.Config.ShopCardDialog;
        if (ImGui.Checkbox("ShopCardDialog", ref shopCard))
        {
            P.Config.ShopCardDialog = shopCard;
            P.Config.Save();
        }

        IndentedTextColored(shadedColor, "Automatically confirm selling Triple Triad cards in the saucer.");

        #endregion
        #region JournalResultComplete

        var journalResultComplete = P.Config.JournalResultCompleteEnabled;
        if (ImGui.Checkbox("JournalResultComplete", ref journalResultComplete))
        {
            P.Config.JournalResultCompleteEnabled = journalResultComplete;
            P.Config.Save();
        }

        IndentedTextColored(shadedColor, "Automatically confirm quest reward acceptance when there is nothing to choose.");

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

        IndentedTextColored(shadedColor, "Automatically commence duties when ready.");

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

        IndentedTextColored(shadedColor, "Automatically commence duties when ready, but only once.\nRequires Contents Finder Confirm, and disables both after activation.");

        #endregion
        #region InclusionShop

        var inclusionShopRemember = P.Config.InclusionShopRememberEnabled;
        if (ImGui.Checkbox("InclusionShopRemember", ref inclusionShopRemember))
        {
            P.Config.InclusionShopRememberEnabled = inclusionShopRemember;
            P.Config.Save();
        }

        IndentedTextColored(shadedColor, "Remember the last panel visited on the scrip exchange window.");

        #endregion
        #region GuildLeveDifficulty

        var guildLeveDifficulty = P.Config.GuildLeveDifficultyConfirm;
        if (ImGui.Checkbox("GuildLeveDifficulty", ref guildLeveDifficulty))
        {
            P.Config.GuildLeveDifficultyConfirm = guildLeveDifficulty;
            P.Config.Save();
        }

        IndentedTextColored(shadedColor, "Automatically confirms guild leves upon initiation at the highest difficulty.");

        #endregion
        #region ShopExchangeItemDialog

        var shopItemExchange = P.Config.ShopExchangeItemDialogEnabled;
        if (ImGui.Checkbox("ShopExchangeItemDialog", ref shopItemExchange))
        {
            P.Config.ShopExchangeItemDialogEnabled = shopItemExchange;
            P.Config.Save();
        }

        IndentedTextColored(shadedColor, "Automatically exchange items/currencies in various shops (e.g. scrip vendors).");

        #endregion
        #region FallGuysRegisterConfirm

        var fgsEnter = P.Config.FallGuysRegisterConfirm;
        if (ImGui.Checkbox("FGSEnterDialog", ref fgsEnter))
        {
            P.Config.FallGuysRegisterConfirm = fgsEnter;
            P.Config.Save();
        }

        IndentedTextColored(shadedColor, "Automatically register for Blunderville when speaking with the Blunderville Registrar.");

        #endregion
        #region FallGuysExitConfirm

        var fgsExit = P.Config.FallGuysExitConfirm;
        if (ImGui.Checkbox("FGSExitDialog", ref fgsExit))
        {
            P.Config.FallGuysExitConfirm = fgsExit;
            P.Config.Save();
        }

        IndentedTextColored(shadedColor, "Automatically confirm the exit prompt when leaving Blunderville.");

        #endregion
        #region FashionCheckQuit

        var fashionQuit = P.Config.FashionCheckQuit;
        if (ImGui.Checkbox("FashionCheck", ref fashionQuit))
        {
            P.Config.FashionCheckQuit = fashionQuit;
            P.Config.Save();
        }

        IndentedTextColored(shadedColor, "Automatically confirm the Fashion Reports results.");

        #endregion
        #region ChocoboRacingQuit

        var chocoboQuit = P.Config.ChocoboRacingQuit;
        if (ImGui.Checkbox("RaceChocoboResult", ref chocoboQuit))
        {
            P.Config.ChocoboRacingQuit = chocoboQuit;
            P.Config.Save();
        }

        IndentedTextColored(shadedColor, "Automatically quit Chocobo Racing when the resuls menu appears.");

        #endregion
        #region LordOfVerminionQuit

        var lovQuit = P.Config.LordOfVerminionQuit;
        if (ImGui.Checkbox("LovmResult", ref lovQuit))
        {
            P.Config.LordOfVerminionQuit = lovQuit;
            P.Config.Save();
        }

        IndentedTextColored(shadedColor, "Automatically quit Lord of Verminion when the results menu appears.");

        #endregion
        #region PartyFinderJoinConfirm

        var pfConfirm = P.Config.PartyFinderJoinConfirm;
        if (ImGui.Checkbox("LookingForGroup x SelectYesno", ref pfConfirm))
        {
            P.Config.PartyFinderJoinConfirm = pfConfirm;
            P.Config.Save();
        }

        IndentedTextColored(shadedColor, "Automatically confirm when joining a party finder group.");

        #endregion

        ImGui.PopID();

        ImGui.EndTabItem();
    }

    // ====================================================================================================

    private static void DisplayTextButtons()
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

    private void DisplayTextNodes()
    {
        var root = RootFolder;
        TextNodeDragDrop(root);

        if (root.Children.Count == 0)
        {
            root.Children.Add(new TextEntryNode() { Enabled = false, Text = "Add some text here!" });
            P.Config.Save();
        }

        foreach (var node in root.Children.ToArray())
        {
            DisplayTextNode(node, root);
        }
    }

    // ====================================================================================================

    private static void DisplayOkButtons()
    {
        var style = ImGui.GetStyle();
        var newStyle = new Vector2(style.ItemSpacing.X / 2, style.ItemSpacing.Y);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newStyle);

        if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.Plus, "Add new entry"))
        {
            var newNode = new OkEntryNode { Enabled = false, Text = "Your text goes here" };
            OkRootFolder.Children.Add(newNode);
            P.Config.Save();
        }

        ImGui.SameLine();
        if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Add last seen as new entry"))
        {
            var io = ImGui.GetIO();
            var createFolder = io.KeyShift;

            Configuration.CreateOkNode(OkRootFolder, createFolder);
            P.Config.Save();
        }

        ImGui.SameLine();
        if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
        {
            var newNode = new TextFolderNode { Name = "Untitled folder" };
            OkRootFolder.Children.Add(newNode);
            P.Config.Save();
        }

        var sb = new StringBuilder();
        sb.AppendLine("Enter into the input all or part of the text inside a dialog.");
        sb.AppendLine("For example: \"You cannot carry any more letters\" for the full mailbox dialog.");
        sb.AppendLine();
        sb.AppendLine("Alternatively, wrap your text in forward slashes to use as a regex.");
        sb.AppendLine("As such: \"/.* carry any more letters .*/\"");
        sb.AppendLine();
        sb.AppendLine("If it matches, the ok button will be clicked.");
        sb.AppendLine();
        sb.AppendLine("Right click a line to view options.");
        sb.AppendLine("Double click an entry for quick enable/disable.");
        sb.AppendLine("Ctrl-Shift right click a line to delete it and any children.");
        sb.AppendLine();
        sb.AppendLine("\"Add last seen as new entry\" button modifiers:");
        sb.AppendLine("   Shift-Click to add to a new or first existing folder.");
        sb.AppendLine();
        sb.AppendLine("Currently supported text addons:");
        sb.AppendLine("  - SelectOk");

        ImGui.SameLine();
        Utils.ImGuiEx.IconButton(FontAwesomeIcon.QuestionCircle, sb.ToString());
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(sb.ToString());

        ImGui.PopStyleVar(); // ItemSpacing
    }

    private void DisplayOkNodes()
    {
        var root = OkRootFolder;
        TextNodeDragDrop(root);

        if (root.Children.Count == 0)
        {
            root.Children.Add(new OkEntryNode() { Enabled = false, Text = "Add some text here!" });
            P.Config.Save();
        }

        foreach (var node in root.Children.ToArray())
        {
            DisplayTextNode(node, root);
        }
    }

    // ====================================================================================================

    private static void DisplayListButtons()
    {
        var style = ImGui.GetStyle();
        var newStyle = new Vector2(style.ItemSpacing.X / 2, style.ItemSpacing.Y);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newStyle);

        if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.Plus, "Add new entry"))
        {
            var newNode = new ListEntryNode { Enabled = false, Text = "Your text goes here" };
            ListRootFolder.Children.Add(newNode);
            P.Config.Save();
        }

        ImGui.SameLine();
        if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Add last selected as new entry"))
        {
            var newNode = new ListEntryNode { Enabled = true, Text = P.LastSeenListSelection, TargetRestricted = true, TargetText = P.LastSeenListTarget };
            ListRootFolder.Children.Add(newNode);
            P.Config.Save();
        }

        ImGui.SameLine();
        if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
        {
            var newNode = new TextFolderNode { Name = "Untitled folder" };
            ListRootFolder.Children.Add(newNode);
            P.Config.Save();
        }

        var sb = new StringBuilder();
        sb.AppendLine("Enter into the input all or part of the text inside a line in a list dialog.");
        sb.AppendLine("For example: \"Purchase a Mini Cactpot ticket\" in the Gold Saucer.");
        sb.AppendLine();
        sb.AppendLine("Alternatively, wrap your text in forward slashes to use as a regex.");
        sb.AppendLine("As such: \"/Purchase a .*? ticket/\"");
        sb.AppendLine();
        sb.AppendLine("If any line in the list matches, then that line will be chosen.");
        sb.AppendLine();
        sb.AppendLine("Right click a line to view options.");
        sb.AppendLine("Double click an entry for quick enable/disable.");
        sb.AppendLine("Ctrl-Shift right click a line to delete it and any children.");
        sb.AppendLine();
        sb.AppendLine("Currently supported list addons:");
        sb.AppendLine("  - SelectString");
        sb.AppendLine("  - SelectIconString");

        ImGui.SameLine();
        Utils.ImGuiEx.IconButton(FontAwesomeIcon.QuestionCircle, sb.ToString());

        ImGui.PopStyleVar(); // ItemSpacing
    }

    private void DisplayListNodes()
    {
        var root = ListRootFolder;
        TextNodeDragDrop(root);

        if (root.Children.Count == 0)
        {
            root.Children.Add(new ListEntryNode() { Enabled = false, Text = "Add some text here!" });
            P.Config.Save();
        }

        foreach (var node in root.Children.ToArray())
        {
            DisplayTextNode(node, root);
        }
    }

    // ====================================================================================================

    private static void DisplayTalkButtons()
    {
        var style = ImGui.GetStyle();
        var newStyle = new Vector2(style.ItemSpacing.X / 2, style.ItemSpacing.Y);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newStyle);

        if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.Plus, "Add new entry"))
        {
            var newNode = new TalkEntryNode { Enabled = false, TargetText = "Your text goes here" };
            TalkRootFolder.Children.Add(newNode);
            P.Config.Save();
        }

        ImGui.SameLine();
        if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Add current target as a new entry"))
        {
            var target = Svc.Targets.Target;
            var targetName = P.LastSeenTalkTarget = target != null
                ? Utils.SEString.GetSeStringText(target.Name)
                : string.Empty;

            var newNode = new TalkEntryNode { Enabled = true, TargetText = targetName };
            TalkRootFolder.Children.Add(newNode);
            P.Config.Save();
        }

        ImGui.SameLine();
        if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
        {
            var newNode = new TextFolderNode { Name = "Untitled folder" };
            TalkRootFolder.Children.Add(newNode);
            P.Config.Save();
        }

        var sb = new StringBuilder();
        sb.AppendLine("Enter into the input all or part of the selected taret name while in a talk dialog.");
        sb.AppendLine("For example: \"Moyce\" in the Crystarium.");
        sb.AppendLine();
        sb.AppendLine("Alternatively, wrap your text in forward slashes to use as a regex.");
        sb.AppendLine("As such: \"/(Moyce|Eirikur)/\"");
        sb.AppendLine();
        sb.AppendLine("To skip your retainers, add the summoning bell.");
        sb.AppendLine();
        sb.AppendLine("Right click a line to view options.");
        sb.AppendLine("Double click an entry for quick enable/disable.");
        sb.AppendLine("Ctrl-Shift right click a line to delete it and any children.");
        sb.AppendLine();
        sb.AppendLine("Currently supported list addons:");
        sb.AppendLine("  - Talk");

        ImGui.SameLine();
        Utils.ImGuiEx.IconButton(FontAwesomeIcon.QuestionCircle, sb.ToString());

        ImGui.PopStyleVar(); // ItemSpacing
    }

    private void DisplayTalkNodes()
    {
        var root = TalkRootFolder;
        TextNodeDragDrop(root);

        if (root.Children.Count == 0)
        {
            root.Children.Add(new TalkEntryNode() { Enabled = false, TargetText = "Add some text here!" });
            P.Config.Save();
        }

        foreach (var node in root.Children.ToArray())
        {
            DisplayTextNode(node, root);
        }
    }

    // ====================================================================================================

    private void DisplayTextNode(ITextNode node, TextFolderNode rootNode)
    {
        if (node is TextFolderNode folderNode)
        {
            DisplayFolderNode(folderNode, rootNode);
        }
        else if (node is TextEntryNode textNode)
        {
            DisplayTextEntryNode(textNode);
        }
        else if (node is OkEntryNode okNode)
        {
            DisplayOkEntryNode(okNode);
        }
        else if (node is ListEntryNode listNode)
        {
            DisplayListEntryNode(listNode);
        }
        else if (node is TalkEntryNode talkNode)
        {
            DisplayTalkEntryNode(talkNode);
        }
    }

    private void DisplayTextEntryNode(TextEntryNode node)
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

        TextNodePopup(node);
        TextNodeDragDrop(node);
    }

    private void DisplayOkEntryNode(OkEntryNode node)
    {
        var validRegex = (node.IsTextRegex && node.TextRegex != null) || !node.IsTextRegex;

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
            Utils.ImGuiEx.TextTooltip("Invalid Text Regex");

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

    private void DisplayListEntryNode(ListEntryNode node)
    {
        var validRegex = (node.IsTextRegex && node.TextRegex != null) || !node.IsTextRegex;
        var validTarget = !node.TargetRestricted || (node.TargetIsRegex && node.TargetRegex != null) || !node.TargetIsRegex;

        if (!node.Enabled && (!validRegex || !validTarget))
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, 0, 0, 1));
        else if (!node.Enabled)
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, .5f, .5f, 1));
        else if (!validRegex || !validTarget)
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));

        ImGui.TreeNodeEx($"{node.Name}##{node.Name}-tree", ImGuiTreeNodeFlags.Leaf);
        ImGui.TreePop();

        if (!node.Enabled || !validRegex || !validTarget)
            ImGui.PopStyleColor();

        if (!validRegex && !validTarget)
            Utils.ImGuiEx.TextTooltip("Invalid Text and Target Regex");
        else if (!validRegex)
            Utils.ImGuiEx.TextTooltip("Invalid Text Regex");
        else if (!validTarget)
            Utils.ImGuiEx.TextTooltip("Invalid Target Regex");

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

    private void DisplayTalkEntryNode(TalkEntryNode node)
    {
        var validTarget = (node.TargetIsRegex && node.TargetRegex != null) || !node.TargetIsRegex;

        if (!node.Enabled && !validTarget)
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, 0, 0, 1));
        else if (!node.Enabled)
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, .5f, .5f, 1));
        else if (!validTarget)
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));

        ImGui.TreeNodeEx($"{node.Name}##{node.Name}-tree", ImGuiTreeNodeFlags.Leaf);
        ImGui.TreePop();

        if (!node.Enabled || !validTarget)
            ImGui.PopStyleColor();

        if (!validTarget)
            Utils.ImGuiEx.TextTooltip("Invalid Target Regex");

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

    private void DisplayFolderNode(TextFolderNode node, TextFolderNode root)
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

    private static void TextNodePopup(ITextNode node, TextFolderNode? root = null)
    {
        var style = ImGui.GetStyle();
        var newItemSpacing = new Vector2(style.ItemSpacing.X / 2, style.ItemSpacing.Y);

        if (ImGui.BeginPopup($"{node.GetHashCode()}-popup"))
        {
            if (node is TextEntryNode entryNode)
            {
                DrawYesNoNode(entryNode, newItemSpacing);
            }

            if (node is OkEntryNode okNode)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newItemSpacing);

                var enabled = okNode.Enabled;
                if (ImGui.Checkbox("Enabled", ref enabled))
                {
                    okNode.Enabled = enabled;
                    P.Config.Save();
                }

                var trashAltWidth = Utils.ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.TrashAlt);

                ImGui.SameLine(ImGui.GetContentRegionMax().X - trashAltWidth);
                if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt, "Delete"))
                {
                    if (P.Config.TryFindParent(node, out var parentNode))
                    {
                        parentNode!.Children.Remove(node);
                        P.Config.Save();
                    }
                }

                var matchText = okNode.Text;
                if (ImGui.InputText($"##{node.Name}-matchText", ref matchText, 10_000, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    okNode.Text = matchText;
                    P.Config.Save();
                }

                ImGui.PopStyleVar(); // ItemSpacing
            }

            if (node is ListEntryNode listNode)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newItemSpacing);

                var enabled = listNode.Enabled;
                if (ImGui.Checkbox("Enabled", ref enabled))
                {
                    listNode.Enabled = enabled;
                    P.Config.Save();
                }

                var trashAltWidth = Utils.ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.TrashAlt);

                ImGui.SameLine(ImGui.GetContentRegionMax().X - trashAltWidth);
                if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt, "Delete"))
                {
                    if (P.Config.TryFindParent(node, out var parentNode))
                    {
                        parentNode!.Children.Remove(node);
                        P.Config.Save();
                    }
                }

                var matchText = listNode.Text;
                if (ImGui.InputText($"##{node.Name}-matchText", ref matchText, 10_000, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    listNode.Text = matchText;
                    P.Config.Save();
                }

                var targetRestricted = listNode.TargetRestricted;
                if (ImGui.Checkbox("Target Restricted", ref targetRestricted))
                {
                    listNode.TargetRestricted = targetRestricted;
                    P.Config.Save();
                }

                var searchPlusWidth = Utils.ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.SearchPlus);

                ImGui.SameLine(ImGui.GetContentRegionMax().X - searchPlusWidth);
                if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Fill with current target"))
                {
                    var target = Svc.Targets.Target;
                    var name = target?.Name?.TextValue ?? string.Empty;

                    if (!string.IsNullOrEmpty(name))
                    {
                        listNode.TargetText = name;
                        P.Config.Save();
                    }
                    else
                    {
                        listNode.TargetText = "Could not find target";
                        P.Config.Save();
                    }
                }

                ImGui.PopStyleVar(); // ItemSpacing

                var targetText = listNode.TargetText;
                if (ImGui.InputText($"##{node.Name}-targetText", ref targetText, 10_000, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    listNode.TargetText = targetText;
                    P.Config.Save();
                }
            }

            if (node is TalkEntryNode talkNode)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newItemSpacing);

                var enabled = talkNode.Enabled;
                if (ImGui.Checkbox("Enabled", ref enabled))
                {
                    talkNode.Enabled = enabled;
                    P.Config.Save();
                }

                var trashAltWidth = Utils.ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.TrashAlt);

                ImGui.SameLine(ImGui.GetContentRegionMax().X - trashAltWidth);
                if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt, "Delete"))
                {
                    if (P.Config.TryFindParent(node, out var parentNode))
                    {
                        parentNode!.Children.Remove(node);
                        P.Config.Save();
                    }
                }

                var searchPlusWidth = Utils.ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.SearchPlus);

                ImGui.SameLine(ImGui.GetContentRegionMax().X - searchPlusWidth - trashAltWidth - newItemSpacing.X);
                if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Fill with current target"))
                {
                    var target = Svc.Targets.Target;
                    var name = target?.Name?.TextValue ?? string.Empty;

                    if (!string.IsNullOrEmpty(name))
                    {
                        talkNode.TargetText = name;
                        P.Config.Save();
                    }
                    else
                    {
                        talkNode.TargetText = "Could not find target";
                        P.Config.Save();
                    }
                }

                ImGui.PopStyleVar(); // ItemSpacing

                var targetText = talkNode.TargetText;
                if (ImGui.InputText($"##{node.Name}-targetText", ref targetText, 10_000, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    talkNode.TargetText = targetText;
                    P.Config.Save();
                }
            }

            if (node is TextFolderNode folderNode)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newItemSpacing);

                if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.Plus, "Add entry"))
                {
                    if (root == RootFolder)
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
                if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Add last seen as new entry"))
                {
                    if (root == RootFolder)
                    {
                        var io = ImGui.GetIO();
                        var zoneRestricted = io.KeyCtrl;
                        var createFolder = io.KeyShift;
                        var selectNo = io.KeyAlt;

                        Configuration.CreateTextNode(folderNode, zoneRestricted, createFolder, selectNo);
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
                if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
                {
                    var newNode = new TextFolderNode { Name = "Untitled folder" };
                    folderNode.Children.Add(newNode);
                    P.Config.Save();
                }

                var trashWidth = Utils.ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.TrashAlt);
                ImGui.SameLine(ImGui.GetContentRegionMax().X - trashWidth);
                if (Utils.ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt, "Delete"))
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

    private static void DrawYesNoNode(TextEntryNode textNode, Vector2 spacing)
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
        if (ImGui.BeginCombo($"##{textNode.Name}-comparisonType", ComparisonTypeToText(comparisonType)))
        {
            foreach (var c in ComparisonTypes)
            {
                var isSelected = comparisonType == c;
                if (ImGui.Selectable(ComparisonTypeToText(c), isSelected))
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

    private static string ComparisonTypeToText(ComparisonType comparisonType)
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

    private void TextNodeDragDrop(ITextNode node)
    {
        if (node != RootFolder && node != ListRootFolder && node != TalkRootFolder && ImGui.BeginDragDropSource())
        {
            draggedNode = node;

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
            if (!nullPtr && payload.IsDelivery() && draggedNode != null)
            {
                if (P.Config.TryFindParent(draggedNode, out var draggedNodeParent))
                {
                    if (targetNode is TextFolderNode targetFolderNode)
                    {
                        draggedNodeParent!.Children.Remove(draggedNode);
                        targetFolderNode.Children.Add(draggedNode);
                        P.Config.Save();
                    }
                    else
                    {
                        if (P.Config.TryFindParent(targetNode, out var targetNodeParent))
                        {
                            var targetNodeIndex = targetNodeParent!.Children.IndexOf(targetNode);
                            if (targetNodeParent == draggedNodeParent)
                            {
                                var draggedNodeIndex = targetNodeParent.Children.IndexOf(draggedNode);
                                if (draggedNodeIndex < targetNodeIndex)
                                {
                                    targetNodeIndex -= 1;
                                }
                            }

                            draggedNodeParent!.Children.Remove(draggedNode);
                            targetNodeParent.Children.Insert(targetNodeIndex, draggedNode);
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
                    throw new Exception($"Could not find parent of node \"{draggedNode.Name}\"");
                }

                draggedNode = null;
            }

            ImGui.EndDragDropTarget();
        }
    }
}
