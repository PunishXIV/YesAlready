using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using System;
using System.Linq;
using YesAlready.Utils;

namespace YesAlready.UI.Tabs;
public static class Bothers
{
    private static readonly string[] hotkeyChoices =
    [
        "None",
        "Control",
        "Alt",
        "Shift",
    ];

    private static readonly VirtualKey[] hotkeyValues =
    [
        VirtualKey.NO_KEY,
        VirtualKey.CONTROL,
        VirtualKey.MENU,
        VirtualKey.SHIFT,
    ];

    public static void Draw()
    {
        using var tab = ImRaii.TabItem("Bothers");
        if (!tab) return;
        using var idScope = ImRaii.PushId($"BothersOptions");

        #region Hotkey Settings

        if (ImGui.CollapsingHeader("Hotkey Settings"))
        {
            // 1. Disable Hotkey
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

            ImGuiEx.IndentedTextColored("While this key is held, the plugin is disabled.");

            // 2. Forced Yes Hotkey
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

            ImGuiEx.IndentedTextColored("2. While this key is held, any Yes/No prompt will always default to yes, and all talk dialogue will be skipped. Be careful.");
        }

        #endregion
        #region Desynthesis/AetherialReduction

        if (ImGui.CollapsingHeader("Desynthesis and Aetherial Reduction"))
        {
            // 3. SalvageDialog
            var desynthDialog = P.Config.DesynthDialogEnabled;
            if (ImGui.Checkbox("SalvageDialog", ref desynthDialog))
            {
                P.Config.DesynthDialogEnabled = desynthDialog;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Remove the Desynthesis menu confirmation.");

            // 4. SalvageDialog (Bulk)
            var desynthBulkDialog = P.Config.DesynthBulkDialogEnabled;
            if (ImGui.Checkbox("SalvageDialog (Bulk)", ref desynthBulkDialog))
            {
                P.Config.DesynthBulkDialogEnabled = desynthBulkDialog;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Check the bulk desynthesis button when using the SalvageDialog feature.");

            // 5. SalvageResults
            var desynthResultsDialog = P.Config.DesynthesisResults;
            if (ImGui.Checkbox("SalvageResults", ref desynthResultsDialog))
            {
                P.Config.DesynthesisResults = desynthResultsDialog;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Automatically closes the SalvageResults window when done desynthesizing.");

            var purifyResultsDialog = P.Config.AetherialReductionResults;
            if (ImGui.Checkbox("PurifyResult", ref purifyResultsDialog))
            {
                P.Config.AetherialReductionResults = purifyResultsDialog;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Automatically closes the PurifyResult window when done reducing.");
        }

        #endregion
        #region Melding

        if (ImGui.CollapsingHeader("Melding"))
        {
            var meld = P.Config.MaterialAttachDialogEnabled;
            if (ImGui.Checkbox("MateriaAttachDialog", ref meld))
            {
                P.Config.MaterialAttachDialogEnabled = meld;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Remove the materia melding confirmation menu.");

            var materialize = P.Config.MaterializeDialogEnabled;
            if (ImGui.Checkbox("MaterializeDialog", ref materialize))
            {
                P.Config.MaterializeDialogEnabled = materialize;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Remove the create new (extract) materia confirmation.");

            var materiaRetrieve = P.Config.MateriaRetrieveDialogEnabled;
            if (ImGui.Checkbox("MateriaRetrieveDialog", ref materiaRetrieve))
            {
                P.Config.MateriaRetrieveDialogEnabled = materiaRetrieve;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Remove the retrieve materia confirmation.");
        }

        #endregion
        #region Retainers & Submersibles

        if (ImGui.CollapsingHeader("Retainers and Submersibles"))
        {
            var retainerTaskAsk = P.Config.RetainerTaskAskEnabled;
            if (ImGui.Checkbox("RetainerTaskAsk", ref retainerTaskAsk))
            {
                P.Config.RetainerTaskAskEnabled = retainerTaskAsk;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Skip the confirmation in the final dialog before sending out a retainer.");

            var retainerTaskResult = P.Config.RetainerTaskResultEnabled;
            if (ImGui.Checkbox("RetainerTaskResult", ref retainerTaskResult))
            {
                P.Config.RetainerTaskResultEnabled = retainerTaskResult;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Automatically send a retainer on the same venture as before when receiving an item.");

            var retainerListDialog = P.Config.RetainerTransferListConfirm;
            if (ImGui.Checkbox("RetainerItemTransferList", ref retainerListDialog))
            {
                P.Config.RetainerTransferListConfirm = retainerListDialog;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Skip the confirmation in the RetainerItemTransferList window to entrust all items to the retainer.");

            var retainerProgressDialog = P.Config.RetainerTransferProgressConfirm;
            if (ImGui.Checkbox("RetainerItemTransferProgress", ref retainerProgressDialog))
            {
                P.Config.RetainerTransferProgressConfirm = retainerProgressDialog;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Automatically closes the RetainerItemTransferProgress window when finished entrusting items.");

            var finalize = P.Config.AirShipExplorationResultFinalize;
            if (ImGui.Checkbox("AirShipExplorationResult - Finalize", ref finalize))
            {
                if (finalize && P.Config.AirShipExplorationResultRedeploy)
                    P.Config.AirShipExplorationResultRedeploy = false;
                P.Config.AirShipExplorationResultFinalize = finalize;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Automatically finalize submersible reports when the AirShipExplorationResult window opens.");

            var redeploy = P.Config.AirShipExplorationResultRedeploy;
            if (ImGui.Checkbox("AirShipExplorationResult - Redeploy", ref redeploy))
            {
                if (redeploy && P.Config.AirShipExplorationResultFinalize)
                    P.Config.AirShipExplorationResultFinalize = false;
                P.Config.AirShipExplorationResultRedeploy = redeploy;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Automatically redeploy submersibles when the AirShipExplorationResult window opens.");
        }

        #endregion
        #region Duties

        if (ImGui.CollapsingHeader("Duties"))
        {
            var contentsFinderConfirm = P.Config.ContentsFinderConfirmEnabled;
            if (ImGui.Checkbox("ContentsFinderConfirm", ref contentsFinderConfirm))
            {
                P.Config.ContentsFinderConfirmEnabled = contentsFinderConfirm;

                if (!contentsFinderConfirm)
                    P.Config.ContentsFinderOneTimeConfirmEnabled = false;

                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Automatically commence duties when ready.");

            var contentsFinderOneTimeConfirm = P.Config.ContentsFinderOneTimeConfirmEnabled;
            if (ImGui.Checkbox("ContentsFinderOneTimeConfirm", ref contentsFinderOneTimeConfirm))
            {
                P.Config.ContentsFinderOneTimeConfirmEnabled = contentsFinderOneTimeConfirm;

                if (contentsFinderOneTimeConfirm)
                    P.Config.ContentsFinderConfirmEnabled = true;

                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Automatically commence duties when ready, but only once. Requires Contents Finder Confirm, and disables both after activation.");
        }

        #endregion
        #region PvP

        if (ImGui.CollapsingHeader("PvP"))
        {
            var ccquit = P.Config.MKSRecordQuit;
            if (ImGui.Checkbox("MKSRecord", ref ccquit))
            {
                P.Config.MKSRecordQuit = ccquit;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Automatically leave the Crystalline Conflict match when the results appear.");

            var flquit = P.Config.FrontlineRecordQuit;
            if (ImGui.Checkbox("FrontlineRecord", ref flquit))
            {
                P.Config.FrontlineRecordQuit = flquit;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Automatically leave the Frontline match when the results appear.");
        }

        #endregion
        #region Gold Saucer

        if (ImGui.CollapsingHeader("Minigames and Special Events"))
        {
            var lotto = P.Config.LotteryWeeklyInput;
            if (ImGui.Checkbox("LotteryWeeklyInput", ref lotto))
            {
                P.Config.LotteryWeeklyInput = lotto;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Automatically purchase a Jumbo Cactpot ticket with a random number.");

            // 19. HWDLottery
            var kupo = P.Config.KupoOfFortune;
            if (ImGui.Checkbox("HWDLottery", ref kupo))
            {
                P.Config.KupoOfFortune = kupo;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Automatically select a kupo of fortune reward. This will instantly complete a single kupo ticket but is unable to continue to the next automatically.");

            var lovQuit = P.Config.LordOfVerminionQuit;
            if (ImGui.Checkbox("LovmResult", ref lovQuit))
            {
                P.Config.LordOfVerminionQuit = lovQuit;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Automatically quit Lord of Verminion when the results menu appears.");

            var fgsEnter = P.Config.FallGuysRegisterConfirm;
            if (ImGui.Checkbox("FGSEnterDialog", ref fgsEnter))
            {
                P.Config.FallGuysRegisterConfirm = fgsEnter;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Automatically register for Blunderville when speaking with the Blunderville Registrar.");

            var fgsExit = P.Config.FallGuysExitConfirm;
            if (ImGui.Checkbox("FGSExitDialog", ref fgsExit))
            {
                P.Config.FallGuysExitConfirm = fgsExit;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Automatically confirm the exit prompt when leaving Blunderville.");

            var fashionQuit = P.Config.FashionCheckQuit;
            if (ImGui.Checkbox("FashionCheck", ref fashionQuit))
            {
                P.Config.FashionCheckQuit = fashionQuit;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Automatically confirm the Fashion Reports results.");

            var chocoboQuit = P.Config.ChocoboRacingQuit;
            if (ImGui.Checkbox("RaceChocoboResult", ref chocoboQuit))
            {
                P.Config.ChocoboRacingQuit = chocoboQuit;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Automatically quit Chocobo Racing when the results menu appears.");

            var shopCard = P.Config.ShopCardDialog;
            if (ImGui.Checkbox("ShopCardDialog", ref shopCard))
            {
                P.Config.ShopCardDialog = shopCard;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Automatically confirm selling Triple Triad cards in the saucer.");
        }

        #endregion
        #region Shops

        if (ImGui.CollapsingHeader("Shops"))
        {
            var inclusionShopRemember = P.Config.InclusionShopRememberEnabled;
            if (ImGui.Checkbox("InclusionShopRemember", ref inclusionShopRemember))
            {
                P.Config.InclusionShopRememberEnabled = inclusionShopRemember;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Remember the last panel visited on the scrip exchange window.");

            var shopItemExchange = P.Config.ShopExchangeItemDialogEnabled;
            if (ImGui.Checkbox("ShopExchangeItemDialog", ref shopItemExchange))
            {
                P.Config.ShopExchangeItemDialogEnabled = shopItemExchange;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Automatically exchange items/currencies in various shops (e.g., scrip vendors).");
        }
        #endregion
        #region Other

        if (ImGui.CollapsingHeader("Other"))
        {
            var deliveries = P.Config.CustomDeliveries;
            if (ImGui.Checkbox("SatisfactionSupply", ref deliveries))
            {
                P.Config.CustomDeliveries = deliveries;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Automatically turn in any available collectibles for Custom Deliveries.");

            var itemInspection = P.Config.ItemInspectionResultEnabled;
            if (ImGui.Checkbox("ItemInspectionResult", ref itemInspection))
            {
                P.Config.ItemInspectionResultEnabled = itemInspection;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Eureka/Bozja lockboxes, forgotten fragments, and more. Warning: this does not check if you are maxed on items. Rate limiter (pause after N items).");

            if (itemInspection)
            {
                ImGui.Indent();
                var rateLimit = P.Config.ItemInspectionResultRateLimiter;
                if (ImGui.InputInt(string.Empty, ref rateLimit))
                {
                    P.Config.ItemInspectionResultRateLimiter = rateLimit;
                    P.Config.Save();
                }
                ImGui.Unindent();
                ImGuiEx.IndentedTextColored("Rate limiter (pause after N items, 0 to disable).");
            }

            var grandCompanySupplyReward = P.Config.GrandCompanySupplyReward;
            if (ImGui.Checkbox("GrandCompanySupplyReward", ref grandCompanySupplyReward))
            {
                P.Config.GrandCompanySupplyReward = grandCompanySupplyReward;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Skip the confirmation when submitting Grand Company expert delivery items.");

            var journalResultComplete = P.Config.JournalResultCompleteEnabled;
            if (ImGui.Checkbox("JournalResultComplete", ref journalResultComplete))
            {
                P.Config.JournalResultCompleteEnabled = journalResultComplete;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Automatically confirm quest reward acceptance when there is nothing to choose.");

            var guildLeveDifficulty = P.Config.GuildLeveDifficultyConfirm;
            if (ImGui.Checkbox("GuildLeveDifficulty", ref guildLeveDifficulty))
            {
                P.Config.GuildLeveDifficultyConfirm = guildLeveDifficulty;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Automatically confirms guild leves upon initiation at the highest difficulty.");

            var dkt = P.Config.DataCentreTravelConfirmEnabled;
            if (ImGui.Checkbox("DataCentreTravelConfirm", ref dkt))
            {
                P.Config.DataCentreTravelConfirmEnabled = dkt;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Automatically accept the Data Center travel confirmation.");

            var mpr = P.Config.MiragePrismRemoveDispel;
            if (ImGui.Checkbox("MiragePrismRemoveDispel", ref mpr))
            {
                P.Config.MiragePrismRemoveDispel = mpr;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Automatically dispel glamours when using Glamour Dispellers.");

            var mpe = P.Config.MiragePrismExecuteCast;
            if (ImGui.Checkbox("MiragePrismExecuteCast", ref mpe))
            {
                P.Config.MiragePrismExecuteCast = mpe;
                P.Config.Save();
            }
            ImGuiEx.IndentedTextColored("Automatically cast glamours when using Glamour Prisms.");
        }

        #endregion
    }
}
