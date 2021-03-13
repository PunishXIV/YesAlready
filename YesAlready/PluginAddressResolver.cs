using Dalamud.Game;
using Dalamud.Game.Internal;
using Dalamud.Plugin;
using System;

namespace YesAlready
{
    internal delegate IntPtr OnSetupDelegate(IntPtr addon, uint a2, IntPtr dataPtr);

    internal class PluginAddressResolver : BaseAddressResolver
    {
        public IntPtr AddonSelectYesNoOnSetupAddress { get; private set; }
        public IntPtr AddonSalvageDialongOnSetupAddress { get; private set; }
        public IntPtr AddonMaterializeDialongOnSetupAddress { get; private set; }

        private const string AddonSelectYesNoOnSetupSignature =
            "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 56 41 57 48 83 EC 40 44 8B F2 0F 29 74 24 ??";  // Client::UI::AddonSelectYesno.OnSetup
        private const string AddonSalvageDialogOnSetupSignature =
            "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 41 56 48 83 EC 30 44 8B F2 49 8B E8";  // Client::UI::AddonSalvageDialog.OnSetup
        private const string AddonMaterializeDialogOnSetupSignature =
            "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 41 56 48 83 EC 50 44 8B F2 49 8B E8 BA ?? ?? ?? ??"; // Client::UI::AddonMaterializeDialog.OnSetup

        protected override void Setup64Bit(SigScanner scanner)
        {
            AddonSelectYesNoOnSetupAddress = scanner.ScanText(AddonSelectYesNoOnSetupSignature);
            AddonSalvageDialongOnSetupAddress = scanner.ScanText(AddonSalvageDialogOnSetupSignature);
            AddonMaterializeDialongOnSetupAddress = scanner.ScanText(AddonMaterializeDialogOnSetupSignature);

            PluginLog.Verbose("===== YES ALREADY =====");
            PluginLog.Verbose($"{nameof(AddonSelectYesNoOnSetupAddress)} {AddonSelectYesNoOnSetupAddress.ToInt64():X}");
            PluginLog.Verbose($"{nameof(AddonSalvageDialongOnSetupAddress)} {AddonSalvageDialongOnSetupAddress.ToInt64():X}");
            PluginLog.Verbose($"{nameof(AddonMaterializeDialongOnSetupAddress)} {AddonMaterializeDialongOnSetupAddress.ToInt64():X}");
        }
    }

}
