using ClickLib;
using Dalamud.Game.Command;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Hooking;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
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
        private Hook<OnSetupDelegate> AddonShopCardDialogOnSetupHook;

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
            OnSetupHooks.Add(AddonShopCardDialogOnSetupHook = new(Address.AddonShopCardDialogOnSetupAddress, new OnSetupDelegate(AddonShopCardDialogOnSetupDetour), this));
            OnSetupHooks.ForEach(hook => hook.Enable());
        }

        public void Dispose()
        {
            Interface.CommandManager.RemoveHandler(Command);

            OnSetupHooks.ForEach(hook => hook.Dispose());

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

                var text = GetSeStringText(GetSeString(zone.Name));
                if (string.IsNullOrEmpty(text))
                    continue;

                TerritoryNames.Add(row.RowId, text);
            }
        }

        internal void PrintMessage(string message) => Interface.Framework.Gui.Chat.Print($"[{Name}] {message}");

        internal void PrintMessage(SeString message)
        {
            message.Payloads.Insert(0, new TextPayload($"[{Name}] "));
            Interface.Framework.Gui.Chat.Print(message);
        }


        internal void PrintError(string message) => Interface.Framework.Gui.Chat.PrintError($"[{Name}] {message}");

        internal void SaveConfiguration() => Interface.SavePluginConfig(Configuration);

        #region SeString

        private unsafe SeString GetSeString(byte* textPtr) => GetSeString((IntPtr)textPtr);

        private SeString GetSeString(IntPtr textPtr)
        {
            var size = 0;
            while (Marshal.ReadByte(textPtr, size) != 0)
                size++;

            var bytes = new byte[size];
            Marshal.Copy(textPtr, bytes, 0, size);

            return GetSeString(bytes);
        }

        private SeString GetSeString(Lumina.Text.SeString luminaString)
        {
            var bytes = Encoding.UTF8.GetBytes(luminaString.RawString);
            return GetSeString(bytes);
        }

        private SeString GetSeString(byte[] bytes)
        {
            return Interface.SeStringManager.Parse(bytes);

        }

        private string GetSeStringText(SeString sestring)
        {
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
            Click.SendClick("select_yes", addon);
        }

        private IntPtr AddonSelectYesNoOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug($"AddonSelectYesNo.OnSetup");
            var result = AddonSelectYesNoOnSetupHook.Original(addon, a2, dataPtr);

            try
            {
                var data = Marshal.PtrToStructure<AddonSelectYesNoOnSetupData>(dataPtr);
                var text = LastSeenDialogText = GetSeStringText(GetSeString(data.textPtr));

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

        private void SendClicks(bool enabled, params string[] clicks)
        {
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
            var result = AddonMaterializeDialogOnSetupHook.Original(addon, a2, dataPtr);
            SendClicks(Configuration.MaterializeDialogEnabled, "materialize");
            return result;
        }

        private IntPtr AddonItemInspectionResultOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug($"AddonItemInspectionResult.OnSetup");

            var result = AddonItemInspectionResultOnSetupHook.Original(addon, a2, dataPtr);

            if (Configuration.ItemInspectionResultEnabled)
            {
                unsafe
                {
                    var addonPtr = (AddonItemInspectionResult*)addon;

                    if (addonPtr->AtkUnitBase.UldManager.NodeListCount >= 64)
                    {
                        var nameNode = (AtkTextNode*)addonPtr->AtkUnitBase.UldManager.NodeList[64];
                        var descNode = (AtkTextNode*)addonPtr->AtkUnitBase.UldManager.NodeList[55];
                        if (nameNode->AtkResNode.IsVisible && descNode->AtkResNode.IsVisible)
                        {
                            var nameText = GetSeString(nameNode->NodeText.StringPtr);
                            var descText = GetSeStringText(GetSeString(descNode->NodeText.StringPtr));
                            if (descText.Contains("※")  // This is hackish, but works well enough (for now)
                                || descText.Contains("liées à Garde-la-Reine"))  // French doesn't have the widget
                            {
                                nameText.Payloads.Insert(0, new TextPayload("Received: "));
                                PrintMessage(nameText);
                            }
                        }
                    }
                }
            }

            SendClicks(Configuration.ItemInspectionResultEnabled, "item_inspection_result_next");

            return result;
        }

        private IntPtr AddonRetainerTaskAskOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug($"AddonRetainerTaskAsk.OnSetup");
            var result = AddonRetainerTaskAskOnSetupHook.Original(addon, a2, dataPtr);
            SendClicks(Configuration.RetainerTaskAskEnabled, "retainer_venture_ask_assign");
            return result;
        }

        private IntPtr AddonRetainerTaskResultOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug($"AddonRetainerTaskResult.OnSetup");
            var result = AddonRetainerTaskResultOnSetupHook.Original(addon, a2, dataPtr);
            SendClicks(Configuration.RetainerTaskResultEnabled, "retainer_venture_result_reassign", "retainer_venture_result_reassign");
            return result;
        }

        private IntPtr AddonGrandCompanySupplyRewardOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug($"AddonGrandCompanySupplyReward.OnSetup");
            var result = AddonGrandCompanySupplyRewardOnSetupHook.Original(addon, a2, dataPtr);
            SendClicks(Configuration.GrandCompanySupplyReward, "grand_company_expert_delivery_deliver");
            return result;
        }

        private IntPtr AddonShopCardDialogOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug($"AddonShopCardDialog.OnSetup");
            var result = AddonShopCardDialogOnSetupHook.Original(addon, a2, dataPtr);
            SendClicks(Configuration.ShopCardDialog, "sell_triple_triad_card");
            return result;
        }

        #endregion

        private void OnChatCommand(string command, string arguments)
        {
            PluginUi.OpenConfig();
        }

    }
}
