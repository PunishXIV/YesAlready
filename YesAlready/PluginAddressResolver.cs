using System;

using Dalamud.Game;
using Dalamud.Logging;

namespace YesAlready
{
    /// <summary>
    /// A delegate matching AtkUnitBase.OnSetup.
    /// </summary>
    /// <param name="addon">Addon address.</param>
    /// <param name="a2">Unused for our purposes.</param>
    /// <param name="dataPtr">Data address.</param>
    /// <returns>The addon address.</returns>
    internal delegate IntPtr OnSetupDelegate(IntPtr addon, uint a2, IntPtr dataPtr);

    /// <summary>
    /// Plugin address resolver.
    /// </summary>
    internal class PluginAddressResolver : BaseAddressResolver
    {
        private const string AddonSelectYesNoOnSetupSignature = // Client::UI::AddonSelectYesno.OnSetup
            "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 56 41 57 48 83 EC 40 44 8B F2 0F 29 74 24 ??";

        private const string AddonSalvageDialogOnSetupSignature = // Client::UI::AddonSalvageDialog.OnSetup
            "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 41 56 48 83 EC 30 44 8B F2 49 8B E8";

        private const string AddonMaterializeDialogOnSetupSignature = // Client::UI::AddonMaterializeDialog.OnSetup
            "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 41 56 48 83 EC 50 44 8B F2 49 8B E8 BA ?? ?? ?? ??";

        private const string AddonMateriaRetrieveDialogOnSetupSignature = // Client::UI::AddonMateriaRetrieveDialog.OnSetup
            "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 30 8B FA 49 8B D8 BA ?? ?? ?? ?? 48 8B F1 E8 ?? ?? ?? ?? 48 8B C8";

        private const string AddonItemInspectionResultOnSetupSignature = // Client::UI::AddonItemInspectionResult.OnSetup
            "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 30 8B F2 49 8B F8 BA ?? ?? ?? ?? 48 8B D9 E8 ?? ?? ?? ?? 48 8B C8 E8 ?? ?? ?? ?? 48 8B D0";

        private const string AddonRetainerTaskAskOnSetupSignature = // Client::UI::AddonRetainerTaskAsk.OnSetup
            "40 53 48 83 EC 30 48 8B D9 83 FA 03 7C 53 49 8B C8 E8 ?? ?? ?? ??";

        private const string AddonRetainerTaskResultOnSetupSignature = // Client::UI::AddonRetainerTaskResult.OnSetup
            "48 89 5C 24 ?? 55 56 57 48 83 EC 40 8B F2 49 8B F8 BA ?? ?? ?? ?? 48 8B D9 E8 ?? ?? ?? ??";

        private const string AddonGrandCompanySupplyRewardOnSetupSignature = // Client::UI::AddonGrandCompanySupplyReward.OnSetup
            "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 54 41 55 41 56 41 57 48 83 EC 30 BA ?? ?? ?? ?? 4D 8B E8 4C 8B F9";

        private const string AddonShopCardDialogOnSetupSignature = // Client::UI::AddonShopCardDialog.OnSetup
            "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 41 54 41 56 41 57 48 83 EC 50 48 8B F9 49 8B F0";

        private const string AddonJournalResultOnSetupSignature = // Client::UI::AddonJournalResult.OnSetup
            "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 30 8B EA 49 8B F0 BA ?? ?? ?? ?? 48 8B F9";

        private const string AddonContentsFinderConfirmOnSetupSignature = // Client::UI::ContentsFinderConfirm.OnSetup
            "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 56 41 57 48 83 EC 30 44 8B F2 49 8B E8 BA ?? ?? ?? ?? 48 8B D9";

        /// <summary>
        /// Gets the address of the SelectYesNo addon's OnSetup method.
        /// </summary>
        public IntPtr AddonSelectYesNoOnSetupAddress { get; private set; }

        /// <summary>
        /// Gets the address of the SalvageDialog addon's OnSetup method.
        /// </summary>
        public IntPtr AddonSalvageDialongOnSetupAddress { get; private set; }

        /// <summary>
        /// Gets the address of the MaterializeDialog addon's OnSetup method.
        /// </summary>
        public IntPtr AddonMaterializeDialongOnSetupAddress { get; private set; }

        /// <summary>
        /// Gets the address of the MateriaRetrieveDialog addon's OnSetup method.
        /// </summary>
        public IntPtr AddonMateriaRetrieveDialongOnSetupAddress { get; private set; }

        /// <summary>
        /// Gets the address of the ItemInspectionResult addon's OnSetup method.
        /// </summary>
        public IntPtr AddonItemInspectionResultOnSetupAddress { get; private set; }

        /// <summary>
        /// Gets the address of the RetainerTaskAsk addon's OnSetup method.
        /// </summary>
        public IntPtr AddonRetainerTaskAskOnSetupAddress { get; private set; }

        /// <summary>
        /// Gets the address of the RetainerTaskResult addon's OnSetup method.
        /// </summary>
        public IntPtr AddonRetainerTaskResultOnSetupAddress { get; private set; }

        /// <summary>
        /// Gets the address of the GrandCompanySupplyReward addon's OnSetup method.
        /// </summary>
        public IntPtr AddonGrandCompanySupplyRewardOnSetupAddress { get; private set; }

        /// <summary>
        /// Gets the address of the ShopCardDialog addon's OnSetup method.
        /// </summary>
        public IntPtr AddonShopCardDialogOnSetupAddress { get; private set; }

        /// <summary>
        /// Gets the address of the JournalResult addon's OnSetup method.
        /// </summary>
        public IntPtr AddonJournalResultOnSetupAddress { get; private set; }

        /// <summary>
        /// Gets the address of the ContentsFinderConfirm addon's OnSetup method.
        /// </summary>
        public IntPtr AddonContentsFinderConfirmOnSetupAddress { get; private set; }

        /// <inheritdoc/>
        protected override void Setup64Bit(SigScanner scanner)
        {
            this.AddonSelectYesNoOnSetupAddress = scanner.ScanText(AddonSelectYesNoOnSetupSignature);
            this.AddonSalvageDialongOnSetupAddress = scanner.ScanText(AddonSalvageDialogOnSetupSignature);
            this.AddonMaterializeDialongOnSetupAddress = scanner.ScanText(AddonMaterializeDialogOnSetupSignature);
            this.AddonMateriaRetrieveDialongOnSetupAddress = scanner.ScanText(AddonMateriaRetrieveDialogOnSetupSignature);
            this.AddonItemInspectionResultOnSetupAddress = scanner.ScanText(AddonItemInspectionResultOnSetupSignature);
            this.AddonRetainerTaskAskOnSetupAddress = scanner.ScanText(AddonRetainerTaskAskOnSetupSignature);
            this.AddonRetainerTaskResultOnSetupAddress = scanner.ScanText(AddonRetainerTaskResultOnSetupSignature);
            this.AddonGrandCompanySupplyRewardOnSetupAddress = scanner.ScanText(AddonGrandCompanySupplyRewardOnSetupSignature);
            this.AddonShopCardDialogOnSetupAddress = scanner.ScanText(AddonShopCardDialogOnSetupSignature);
            this.AddonJournalResultOnSetupAddress = scanner.ScanText(AddonJournalResultOnSetupSignature);
            this.AddonContentsFinderConfirmOnSetupAddress = scanner.ScanText(AddonContentsFinderConfirmOnSetupSignature);

            PluginLog.Verbose("===== YES ALREADY =====");
            PluginLog.Verbose($"{nameof(this.AddonSelectYesNoOnSetupAddress)} {this.AddonSelectYesNoOnSetupAddress:X}");
            PluginLog.Verbose($"{nameof(this.AddonSalvageDialongOnSetupAddress)} {this.AddonSalvageDialongOnSetupAddress:X}");
            PluginLog.Verbose($"{nameof(this.AddonMaterializeDialongOnSetupAddress)} {this.AddonMaterializeDialongOnSetupAddress:X}");
            PluginLog.Verbose($"{nameof(this.AddonMateriaRetrieveDialongOnSetupAddress)} {this.AddonMateriaRetrieveDialongOnSetupAddress:X}");
            PluginLog.Verbose($"{nameof(this.AddonItemInspectionResultOnSetupAddress)} {this.AddonItemInspectionResultOnSetupAddress:X}");
            PluginLog.Verbose($"{nameof(this.AddonRetainerTaskAskOnSetupAddress)} {this.AddonRetainerTaskAskOnSetupAddress:X}");
            PluginLog.Verbose($"{nameof(this.AddonRetainerTaskResultOnSetupAddress)} {this.AddonRetainerTaskResultOnSetupAddress:X}");
            PluginLog.Verbose($"{nameof(this.AddonGrandCompanySupplyRewardOnSetupAddress)} {this.AddonGrandCompanySupplyRewardOnSetupAddress:X}");
            PluginLog.Verbose($"{nameof(this.AddonShopCardDialogOnSetupAddress)} {this.AddonShopCardDialogOnSetupAddress:X}");
            PluginLog.Verbose($"{nameof(this.AddonJournalResultOnSetupAddress)} {this.AddonJournalResultOnSetupAddress:X}");
            PluginLog.Verbose($"{nameof(this.AddonContentsFinderConfirmOnSetupAddress)} {this.AddonContentsFinderConfirmOnSetupAddress:X}");
        }
    }
}
