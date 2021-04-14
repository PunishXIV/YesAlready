using ClickLib;
using Dalamud.Game.Command;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Hooking;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.UI;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace YesAlready
{
    public class YesAlreadyPlugin : IDalamudPlugin
    {
        public string Name => "YesAlready";
        public string Command => "/pyes";

        internal const int CURRENT_CONFIG_VERSION = 2;
        internal YesAlreadyConfiguration Configuration;
        internal DalamudPluginInterface Interface;
        internal PluginAddressResolver Address;
        private PluginUI PluginUi;

        private readonly List<Hook<OnSetupDelegate>> OnSetupHooks = new();
        private Hook<OnSetupDelegate> AddonSelectYesNoOnSetupHook;
        private Hook<OnSetupDelegate> AddonSalvageDialogOnSetupHook;
        private Hook<OnSetupDelegate> AddonMaterializeDialogOnSetupHook;
        private Hook<OnSetupDelegate> AddonItemInspectionResultOnSetupHook;
        private Hook<OnSetupDelegate> AddonRetainerTaskAskOnSetupHook;
        private Hook<OnSetupDelegate> AddonRetainerTaskResultOnSetupHook;
        private Hook<OnSetupDelegate> AddonGrandCompanySupplyRewardOnSetupHook;
        private Hook<AddonTalkVf46Delegate> AddonTalkVf46Hook;

        internal readonly Dictionary<uint, string> TerritoryNames = new();

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            Interface = pluginInterface ?? throw new ArgumentNullException(nameof(pluginInterface), "DalamudPluginInterface cannot be null");

            Configuration = YesAlreadyConfiguration.Load(pluginInterface);
            if (Configuration.Version < CURRENT_CONFIG_VERSION)
            {
                Configuration.Upgrade();
                SaveConfiguration();
            }

            Interface.CommandManager.AddHandler(Command, new CommandInfo(OnChatCommand)
            {
                HelpMessage = "Open a window to edit various settings.",
                ShowInHelp = true
            });

            Address = new PluginAddressResolver();
            Address.Setup(pluginInterface.TargetModuleScanner);

            LoadTerritories();

            PluginUi = new PluginUI(this);

            Click.Initialize(pluginInterface);

            OnSetupHooks.Add(AddonSelectYesNoOnSetupHook = new(Address.AddonSelectYesNoOnSetupAddress, new OnSetupDelegate(AddonSelectYesNoOnSetupDetour), this));
            OnSetupHooks.Add(AddonSalvageDialogOnSetupHook = new(Address.AddonSalvageDialongOnSetupAddress, new OnSetupDelegate(AddonSalvageDialogOnSetupDetour), this));
            OnSetupHooks.Add(AddonMaterializeDialogOnSetupHook = new(Address.AddonMaterializeDialongOnSetupAddress, new OnSetupDelegate(AddonMaterializeDialogOnSetupDetour), this));
            OnSetupHooks.Add(AddonItemInspectionResultOnSetupHook = new(Address.AddonItemInspectionResultOnSetupAddress, new OnSetupDelegate(AddonItemInspectionResultOnSetupDetour), this));
            OnSetupHooks.Add(AddonRetainerTaskAskOnSetupHook = new(Address.AddonRetainerTaskAskOnSetupAddress, new OnSetupDelegate(AddonRetainerTaskAskOnSetupDetour), this));
            OnSetupHooks.Add(AddonRetainerTaskResultOnSetupHook = new(Address.AddonRetainerTaskResultOnSetupAddress, new OnSetupDelegate(AddonRetainerTaskResultOnSetupDetour), this));
            OnSetupHooks.Add(AddonGrandCompanySupplyRewardOnSetupHook = new(Address.AddonGrandCompanySupplyRewardOnSetupAddress, new OnSetupDelegate(AddonGrandCompanySupplyRewardOnSetupDetour), this));
            OnSetupHooks.ForEach(hook => hook.Enable());

#if DEBUG
            AddonTalkVf46Hook = new(Address.AddonTalkVf46Address, new AddonTalkVf46Delegate(AddonTalkVf46Detour), this);
            AddonTalkVf46Hook.Enable();
#endif
        }

        public void Dispose()
        {
            Interface.CommandManager.RemoveHandler(Command);

            OnSetupHooks.ForEach(hook => hook.Dispose());
            AddonTalkVf46Hook?.Dispose();

            PluginUi.Dispose();
        }

        private void LoadTerritories()
        {
            var sheet = Interface.Data.GetExcelSheet<TerritoryType>();
            foreach (var row in sheet)
            {
                var zone = row.PlaceName.Value;
                if (zone == null)
                    continue;

                var text = GetSeStringText(zone.Name);
                if (string.IsNullOrEmpty(text))
                    continue;

                TerritoryNames.Add(row.RowId, text);
            }
        }

        internal void PrintMessage(string message) => Interface.Framework.Gui.Chat.Print($"[{Name}] {message}");

        internal void PrintError(string message) => Interface.Framework.Gui.Chat.PrintError($"[{Name}] {message}");

        internal void SaveConfiguration() => Interface.SavePluginConfig(Configuration);

        #region SeString

        private string GetSeStringText(IntPtr textPtr)
        {
            var size = 0;
            while (Marshal.ReadByte(textPtr, size) != 0)
                size++;

            var bytes = new byte[size];
            Marshal.Copy(textPtr, bytes, 0, size);

            return GetSeStringText(bytes);
        }

        private string GetSeStringText(Lumina.Text.SeString luminaString)
        {
            var bytes = Encoding.UTF8.GetBytes(luminaString.RawString);
            return GetSeStringText(bytes);
        }

        private string GetSeStringText(byte[] bytes)
        {
            var sestring = Interface.SeStringManager.Parse(bytes);
            var pieces = sestring.Payloads.OfType<TextPayload>().Select(t => t.Text);
            var text = string.Join("", pieces).Replace('\n', ' ').Trim();
            return text;
        }

        #endregion

        #region YesNo

        internal string LastSeenDialogText { get; set; } = "";

        [StructLayout(LayoutKind.Explicit, Size = 0x10)]
        private struct AddonSelectYesNoOnSetupData
        {
            [FieldOffset(0x8)] public IntPtr textPtr;
        }

        private bool EntryMatchesText(TextEntryNode node, string text)
        {
            return (node.IsTextRegex && (node.TextRegex?.IsMatch(text) ?? false)) ||
                  (!node.IsTextRegex && text.Contains(node.Text));
        }

        private bool EntryMatchesZoneName(TextEntryNode node, string zoneName)
        {
            return (node.ZoneIsRegex && (node.ZoneRegex?.IsMatch(zoneName) ?? false)) ||
                  (!node.ZoneIsRegex && zoneName.Contains(node.ZoneText));
        }

        private void AddonSelectYesNoExecute(IntPtr addon)
        {
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
        }

        private IntPtr AddonSelectYesNoOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug($"AddonSelectYesNo.OnSetup");
            var result = AddonSelectYesNoOnSetupHook.Original(addon, a2, dataPtr);

            try
            {
                var data = Marshal.PtrToStructure<AddonSelectYesNoOnSetupData>(dataPtr);
                var text = LastSeenDialogText = GetSeStringText(data.textPtr);

                PluginLog.Debug($"AddonSelectYesNo: text={text}");

                if (Configuration.Enabled)
                {
                    var nodes = Configuration.GetAllNodes().OfType<TextEntryNode>();
                    var zoneWarnOnce = true;
                    foreach (var node in nodes)
                    {
                        if (node.Enabled && !string.IsNullOrEmpty(node.Text) && EntryMatchesText(node, text))
                        {
                            if (node.ZoneRestricted && !string.IsNullOrEmpty(node.ZoneText))
                            {
                                if (!TerritoryNames.TryGetValue(Interface.ClientState.TerritoryType, out var zoneName))
                                {
                                    if (zoneWarnOnce && !(zoneWarnOnce = false))
                                    {
                                        PluginLog.Debug("Unable to verify Zone Restricted entry, ZoneID was not set yet");
                                        PrintMessage($"Unable to verify Zone Restricted entry, change zones to update value");
                                    }
                                    zoneName = "";
                                }

                                if (!string.IsNullOrEmpty(zoneName) && EntryMatchesZoneName(node, zoneName))
                                {
                                    PluginLog.Debug($"AddonSelectYesNo: Matched on {node.Text} ({node.ZoneText})");
                                    AddonSelectYesNoExecute(addon);
                                    break;
                                }
                            }
                            else
                            {
                                PluginLog.Debug($"AddonSelectYesNo: Matched on {node.Text}");
                                AddonSelectYesNoExecute(addon);
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

        #endregion

        #region Non-text matching

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

            var result = AddonSalvageDialogOnSetupHook.Original(addon, a2, dataPtr);

            try
            {
                if (Configuration.Enabled && Configuration.DesynthBulkDialogEnabled)
                {
                    unsafe
                    {
                        ((AddonSalvageDialog*)addon)->BulkDesynthEnabled = true;
                    }
                }

                if (Configuration.Enabled && Configuration.DesynthDialogEnabled)
                {
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

        private IntPtr AddonGrandCompanySupplyRewardOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug($"AddonGrandCompanySupplyReward.OnSetup");
            return AddonNoTextMatchDetour(addon, a2, dataPtr, AddonGrandCompanySupplyRewardOnSetupHook, Configuration.GrandCompanySupplyReward, "grand_company_expert_delivery_deliver");
        }

        #endregion

        #region Talk

        private delegate byte AddonTalkVf46Delegate(IntPtr addon, long a2, IntPtr dataPtr);

        private byte AddonTalkVf46Detour(IntPtr addon, long a2, IntPtr dataPtr)
        {
            PluginLog.Information($"AddonTalk.vf46 {addon.ToInt64():X} {a2:X} {dataPtr.ToInt64():X}");

            var stringPtrPtr = dataPtr + 0x8;
            var originalStringPtr = IntPtr.Zero;
            var newStringPtr = IntPtr.Zero;

            try
            {
                if (Configuration.Enabled)
                {
                    originalStringPtr = Marshal.ReadIntPtr(stringPtrPtr);
                    var originalText = GetSeStringText(originalStringPtr);
                    PluginLog.Information($"Original={originalText}");

                    var bytes = Encoding.UTF8.GetBytes($"Skip this text. {new Random().Next(1, 1000)}\0");
                    newStringPtr = Marshal.AllocHGlobal(bytes.Length);
                    Marshal.Copy(bytes, 0, newStringPtr, bytes.Length);
                    Marshal.WriteIntPtr(stringPtrPtr, newStringPtr);


                    //var data = Marshal.PtrToStructure<AddonSelectYesNoOnSetupData>(dataPtr);
                    //var text = LastSeenDialogText = GetSeStringText(data.textPtr);

                    //PluginLog.Debug($"AddonSelectYesNo text={text}");
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Don't crash the game");
            }

            var result = AddonTalkVf46Hook.Original(addon, a2, dataPtr);

            unsafe
            {
                var addonObj = (AddonTalk*)addon;
                //PluginLog.Information($"Talk is visible ? {addonObj->AtkUnitBase.IsVisible}");
            }

            try
            {
                if (originalStringPtr != IntPtr.Zero)
                    Marshal.WriteIntPtr(stringPtrPtr, originalStringPtr);

                if (newStringPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(newStringPtr);
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Don't crash the game");
            }

            return result;
        }

        #endregion

        private void OnChatCommand(string command, string arguments)
        {
            PluginUi.OpenConfig();
        }

    }
}
