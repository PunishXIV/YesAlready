using ClickLib;
using Dalamud.Game.Chat.SeStringHandling;
using Dalamud.Game.Chat.SeStringHandling.Payloads;
using Dalamud.Game.Command;
using Dalamud.Hooking;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.UI;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace YesAlready
{
    public class YesAlreadyPlugin : IDalamudPlugin
    {
        public string Name => "YesAlready";
        public string Command => "/pyes";

        internal YesAlreadyConfiguration Configuration;
        internal DalamudPluginInterface Interface;
        internal PluginAddressResolver Address;
        private PluginUI PluginUi;

        private Hook<OnSetupDelegate> AddonSelectYesNoOnSetupHook;
        private Hook<OnSetupDelegate> AddonSalvageDialogOnSetupHook;
        private Hook<OnSetupDelegate> AddonMaterializeDialogOnSetupHook;
        private Hook<OnSetupDelegate> AddonItemInspectionResultOnSetupHook;
        private Hook<OnSetupDelegate> AddonRetainerTaskAskOnSetupHook;
        private Hook<OnSetupDelegate> AddonRetainerTaskResultOnSetupHook;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            Interface = pluginInterface ?? throw new ArgumentNullException(nameof(pluginInterface), "DalamudPluginInterface cannot be null");
            Configuration = pluginInterface.GetPluginConfig() as YesAlreadyConfiguration ?? new YesAlreadyConfiguration();

            Interface.CommandManager.AddHandler(Command, new CommandInfo(OnChatCommand)
            {
                HelpMessage = "Open a window to edit various settings.",
                ShowInHelp = true
            });

            Address = new PluginAddressResolver();
            Address.Setup(pluginInterface.TargetModuleScanner);
            PluginUi = new PluginUI(this);

            Click.Initialize(pluginInterface);

            AddonSelectYesNoOnSetupHook = new(Address.AddonSelectYesNoOnSetupAddress, new OnSetupDelegate(AddonSelectYesNoOnSetupDetour), this);
            AddonSelectYesNoOnSetupHook.Enable();

            AddonSalvageDialogOnSetupHook = new(Address.AddonSalvageDialongOnSetupAddress, new OnSetupDelegate(AddonSalvageDialogOnSetupDetour), this);
            AddonSalvageDialogOnSetupHook.Enable();

            AddonMaterializeDialogOnSetupHook = new(Address.AddonMaterializeDialongOnSetupAddress, new OnSetupDelegate(AddonMaterializeDialogOnSetupDetour), this);
            AddonMaterializeDialogOnSetupHook.Enable();

            AddonItemInspectionResultOnSetupHook = new(Address.AddonItemInspectionResultOnSetupAddress, new OnSetupDelegate(AddonItemInspectionResultOnSetupDetour), this);
            AddonItemInspectionResultOnSetupHook.Enable();

            AddonRetainerTaskAskOnSetupHook = new(Address.AddonRetainerTaskAskOnSetupAddress, new OnSetupDelegate(AddonRetainerTaskAskOnSetupDetour), this);
            AddonRetainerTaskAskOnSetupHook.Enable();

            AddonRetainerTaskResultOnSetupHook = new(Address.AddonRetainerTaskResultOnSetupAddress, new OnSetupDelegate(AddonRetainerTaskResultOnSetupDetour), this);
            AddonRetainerTaskResultOnSetupHook.Enable();
        }

        public void Dispose()
        {
            Interface.CommandManager.RemoveHandler(Command);

            AddonSelectYesNoOnSetupHook.Dispose();
            AddonSalvageDialogOnSetupHook.Dispose();
            AddonMaterializeDialogOnSetupHook.Dispose();
            AddonItemInspectionResultOnSetupHook.Dispose();
            AddonRetainerTaskAskOnSetupHook.Dispose();
            AddonRetainerTaskResultOnSetupHook.Dispose();

            PluginUi.Dispose();
        }

        internal void PrintMessage(string message) => Interface.Framework.Gui.Chat.Print(message);

        internal void PrintError(string message) => Interface.Framework.Gui.Chat.PrintError(message);

        internal void SaveConfiguration() => Interface.SavePluginConfig(Configuration);

        internal string LastSeenDialogText { get; set; } = "";

        private string GetSeStringText(IntPtr textPtr)
        {
            var size = 0;
            while (Marshal.ReadByte(textPtr, size) != 0)
                size++;

            var bytes = new byte[size];
            Marshal.Copy(textPtr, bytes, 0, size);

            var sestring = Interface.SeStringManager.Parse(bytes);
            var pieces = sestring.Payloads.OfType<TextPayload>().Select(t => t.Text);
            var text = string.Join("", pieces).Replace('\n', ' ');
            return text;
        }

        [StructLayout(LayoutKind.Explicit, Size = 0x10)]
        private struct AddonSelectYesNoOnSetupData
        {
            [FieldOffset(0x8)] public IntPtr textPtr;
        }

        private IntPtr AddonSelectYesNoOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug($"AddonSelectYesNo.OnSetup");
            var result = AddonSelectYesNoOnSetupHook.Original(addon, a2, dataPtr);

            try
            {
                var data = Marshal.PtrToStructure<AddonSelectYesNoOnSetupData>(dataPtr);
                var text = LastSeenDialogText = GetSeStringText(data.textPtr);

                PluginLog.Debug($"AddonSelectYesNo text={text}");

                if (Configuration.Enabled)
                {
                    foreach (var item in Configuration.TextEntries)
                    {
                        if (item.Enabled && !string.IsNullOrEmpty(item.Text))
                        {
                            if ((item.IsRegex && (item.Regex?.IsMatch(text) ?? false)) ||
                                (!item.IsRegex && text.Contains(item.Text)))
                            {
                                PluginLog.Debug($"AddonSelectYesNo: Matched on {item.Text}");
                                unsafe
                                {
                                    var addonObj = (AddonSelectYesno*)addon;
                                    var yesButton = addonObj->YesButton;
                                    if (yesButton != null && !yesButton->IsEnabled)
                                    {
                                        PluginLog.Debug($"AddonSelectYesNo: Enabling yes button");
                                        yesButton->AtkComponentBase.OwnerNode->AtkResNode.Flags ^= 1 << 5;
                                    }
                                }

                                PluginLog.Debug($"AddonSelectYesNo: Selecting yes");
                                Click.SendClick("select_yes");
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Don't crash the game");
            }

            return result;
        }

        private IntPtr AddonNoTextMatchDetour(IntPtr addon, uint a2, IntPtr dataPtr, Hook<OnSetupDelegate> hook, bool enabled, params string[] clicks)
        {
            var result = hook.Original(addon, a2, dataPtr);

            try
            {
                if (Configuration.Enabled && enabled)
                    foreach (var click in clicks)
                        Click.SendClick(click);
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Don't crash the game");
            }

            return result;
        }

        private IntPtr AddonSalvageDialogOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug($"AddonSalvageDialog.OnSetup");
            return AddonNoTextMatchDetour(addon, a2, dataPtr, AddonSalvageDialogOnSetupHook, Configuration.DesynthDialogEnabled, "desynthesize_checkbox", "desynthesize");
        }

        private IntPtr AddonMaterializeDialogOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug($"AddonMaterializeDialog.OnSetupDetour");
            return AddonNoTextMatchDetour(addon, a2, dataPtr, AddonMaterializeDialogOnSetupHook, Configuration.MaterializeDialogEnabled, "materialize");
        }

        private IntPtr AddonItemInspectionResultOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug($"AddonItemInspectionResult.OnSetup");
            return AddonNoTextMatchDetour(addon, a2, dataPtr, AddonItemInspectionResultOnSetupHook, Configuration.ItemInspectionResultEnabled, "item_inspection_result_next");
        }

        private IntPtr AddonRetainerTaskAskOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug($"AddonRetainerTaskAsk.OnSetup");
            return AddonNoTextMatchDetour(addon, a2, dataPtr, AddonRetainerTaskAskOnSetupHook, Configuration.RetainerTaskAskEnabled, "retainer_venture_ask_assign");
        }

        private IntPtr AddonRetainerTaskResultOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug($"AddonRetainerTaskResult.OnSetup");
            return AddonNoTextMatchDetour(addon, a2, dataPtr, AddonRetainerTaskResultOnSetupHook, Configuration.RetainerTaskResultEnabled, "retainer_venture_result_reassign");
        }

        private void OnChatCommand(string command, string arguments)
        {
            PluginUi.Open();
        }
    }
}
