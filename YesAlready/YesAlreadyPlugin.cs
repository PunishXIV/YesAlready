using Clicklib;
using Dalamud.Game.Command;
using Dalamud.Hooking;
using Dalamud.Plugin;
using System;
using System.Runtime.InteropServices;

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
        }

        public void Dispose()
        {
            Interface.CommandManager.RemoveHandler(Command);

            AddonSelectYesNoOnSetupHook.Dispose();
            AddonSalvageDialogOnSetupHook.Dispose();
            AddonMaterializeDialogOnSetupHook.Dispose();

            PluginUi.Dispose();
        }

        internal void SaveConfiguration() => Interface.SavePluginConfig(Configuration);

        internal string LastSeenDialogText { get; set; } = "";

        [StructLayout(LayoutKind.Explicit, Size = 0x10)]
        private struct AddonSelectYesNoOnSetupData
        {
            [FieldOffset(0x8)] public IntPtr textPtr;
        }

        private IntPtr AddonSelectYesNoOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            var result = AddonSelectYesNoOnSetupHook.Original(addon, a2, dataPtr);

            try
            {
                var data = Marshal.PtrToStructure<AddonSelectYesNoOnSetupData>(dataPtr);
                var text = LastSeenDialogText = Marshal.PtrToStringAnsi(data.textPtr);

                if (Configuration.Enabled)
                {
                    foreach (var item in Configuration.TextEntries)
                    {
                        if (item.Enabled && !string.IsNullOrEmpty(item.Text))
                        {
                            if ((item.IsRegex && (item.Regex?.IsMatch(text) ?? false)) ||
                                (!item.IsRegex && text.Contains(item.Text)))
                            {
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

        private IntPtr AddonSalvageDialogOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            var result = AddonSalvageDialogOnSetupHook.Original(addon, a2, dataPtr);

            try
            {
                if (Configuration.Enabled && Configuration.DesynthDialogEnabled)
                {
                    // It doesn't seem to matter if the checkbox is visible or not
                    Click.SendClick("desynthesize_checkbox");
                    Click.SendClick("desynthesize");
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Don't crash the game");
            }

            return result;
        }

        private IntPtr AddonMaterializeDialogOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            var result = AddonMaterializeDialogOnSetupHook.Original(addon, a2, dataPtr);

            try
            {
                if (Configuration.Enabled && Configuration.MaterializeDialogEnabled)
                {
                    Click.SendClick("materialize");
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Don't crash the game");
            }

            return result;
        }

        private void OnChatCommand(string command, string arguments)
        {
            PluginUi.Open();
        }
    }
}
