using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using ClickLib;
using ClickLib.Clicks;
using Dalamud.Game.Command;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Hooking;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using Dalamud.Memory;
using Dalamud.Plugin;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace YesAlready
{
    /// <summary>
    /// Main plugin implementation.
    /// </summary>
    public sealed partial class YesAlreadyPlugin : IDalamudPlugin
    {
        private const int CurrentConfigVersion = 2;
        private const string Command = "/pyes";

        private readonly WindowSystem windowSystem;
        private readonly ConfigWindow configWindow;
        private readonly ZoneListWindow zoneListWindow;

        private readonly List<Hook<OnSetupDelegate>> onSetupHooks = new();
        private readonly Hook<OnSetupDelegate> addonSelectYesNoOnSetupHook;
        private readonly Hook<OnSetupDelegate> addonSalvageDialogOnSetupHook;
        private readonly Hook<OnSetupDelegate> addonMaterializeDialogOnSetupHook;
        private readonly Hook<OnSetupDelegate> addonMateriaRetrieveDialogOnSetupHook;
        private readonly Hook<OnSetupDelegate> addonItemInspectionResultOnSetupHook;
        private readonly Hook<OnSetupDelegate> addonRetainerTaskAskOnSetupHook;
        private readonly Hook<OnSetupDelegate> addonRetainerTaskResultOnSetupHook;
        private readonly Hook<OnSetupDelegate> addonGrandCompanySupplyRewardOnSetupHook;
        private readonly Hook<OnSetupDelegate> addonShopCardDialogOnSetupHook;
        private readonly Hook<OnSetupDelegate> addonJournalResultOnSetupHook;
        private readonly Hook<OnSetupDelegate> addonContentsFinderConfirmOnSetupHook;

        /// <summary>
        /// Initializes a new instance of the <see cref="YesAlreadyPlugin"/> class.
        /// </summary>
        /// <param name="pluginInterface">Dalamud plugin interface.</param>
        public YesAlreadyPlugin(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Service>();

            Service.Plugin = this;

            Service.Configuration = YesAlreadyConfiguration.Load(pluginInterface.ConfigDirectory);
            if (Service.Configuration.Version < CurrentConfigVersion)
            {
                Service.Configuration.Upgrade();
                Service.Configuration.Save();
            }

            Service.Address = new PluginAddressResolver();
            Service.Address.Setup();

            this.LoadTerritories();
            Click.Initialize();

            this.onSetupHooks.Add(this.addonSelectYesNoOnSetupHook = new(Service.Address.AddonSelectYesNoOnSetupAddress, this.AddonSelectYesNoOnSetupDetour));
            this.onSetupHooks.Add(this.addonSalvageDialogOnSetupHook = new(Service.Address.AddonSalvageDialongOnSetupAddress, this.AddonSalvageDialogOnSetupDetour));
            this.onSetupHooks.Add(this.addonMaterializeDialogOnSetupHook = new(Service.Address.AddonMaterializeDialongOnSetupAddress, this.AddonMaterializeDialogOnSetupDetour));
            this.onSetupHooks.Add(this.addonMateriaRetrieveDialogOnSetupHook = new(Service.Address.AddonMateriaRetrieveDialongOnSetupAddress, this.AddonMateriaRetrieveDialogOnSetupDetour));
            this.onSetupHooks.Add(this.addonItemInspectionResultOnSetupHook = new(Service.Address.AddonItemInspectionResultOnSetupAddress, this.AddonItemInspectionResultOnSetupDetour));
            this.onSetupHooks.Add(this.addonRetainerTaskAskOnSetupHook = new(Service.Address.AddonRetainerTaskAskOnSetupAddress, this.AddonRetainerTaskAskOnSetupDetour));
            this.onSetupHooks.Add(this.addonRetainerTaskResultOnSetupHook = new(Service.Address.AddonRetainerTaskResultOnSetupAddress, this.AddonRetainerTaskResultOnSetupDetour));
            this.onSetupHooks.Add(this.addonGrandCompanySupplyRewardOnSetupHook = new(Service.Address.AddonGrandCompanySupplyRewardOnSetupAddress, this.AddonGrandCompanySupplyRewardOnSetupDetour));
            this.onSetupHooks.Add(this.addonShopCardDialogOnSetupHook = new(Service.Address.AddonShopCardDialogOnSetupAddress, this.AddonShopCardDialogOnSetupDetour));
            this.onSetupHooks.Add(this.addonJournalResultOnSetupHook = new(Service.Address.AddonJournalResultOnSetupAddress, this.AddonJournalResultOnSetupDetour));
            this.onSetupHooks.Add(this.addonContentsFinderConfirmOnSetupHook = new(Service.Address.AddonContentsFinderConfirmOnSetupAddress, this.AddonContentsFinderConfirmOnSetupDetour));
            this.onSetupHooks.ForEach(hook => hook.Enable());

            this.configWindow = new();
            this.zoneListWindow = new();
            this.windowSystem = new("Yes Already");
            this.windowSystem.AddWindow(this.configWindow);
            this.windowSystem.AddWindow(this.zoneListWindow);

            Service.Interface.UiBuilder.Draw += this.windowSystem.Draw;
            Service.Interface.UiBuilder.OpenConfigUi += this.OnOpenConfigUi;

            Service.CommandManager.AddHandler(Command, new CommandInfo(this.OnChatCommand)
            {
                HelpMessage = "Open a window to edit various settings.",
                ShowInHelp = true,
            });
        }

        /// <inheritdoc/>
        public string Name => "Yes Already";

        /// <summary>
        /// Gets a mapping of territory IDs to names.
        /// </summary>
        internal Dictionary<uint, string> TerritoryNames { get; } = new();

        /// <summary>
        /// Gets or sets the text of the last seen dialog.
        /// </summary>
        internal string LastSeenDialogText { get; set; } = string.Empty;

        /// <inheritdoc/>
        public void Dispose()
        {
            Service.CommandManager.RemoveHandler(Command);

            Service.Interface.UiBuilder.OpenConfigUi -= this.OnOpenConfigUi;
            Service.Interface.UiBuilder.Draw -= this.windowSystem.Draw;

            this.onSetupHooks.ForEach(hook => hook.Dispose());
        }

        /// <summary>
        /// Print a message to the chat window.
        /// </summary>
        /// <param name="message">Message to display.</param>
        internal void PrintMessage(string message)
        {
            Service.ChatGui.Print($"[{this.Name}] {message}");
        }

        /// <summary>
        /// Print a message to the chat window.
        /// </summary>
        /// <param name="message">Message to display.</param>
        internal void PrintMessage(SeString message)
        {
            message.Payloads.Insert(0, new TextPayload($"[{this.Name}] "));
            Service.ChatGui.Print(message);
        }

        /// <summary>
        /// Print an error message to the chat window.
        /// </summary>
        /// <param name="message">Message to display.</param>
        internal void PrintError(string message)
        {
            Service.ChatGui.PrintError($"[{this.Name}] {message}");
        }

        /// <summary>
        /// Opens the zone list window.
        /// </summary>
        internal void OpenZoneListUi() => this.zoneListWindow.IsOpen = true;

        /// <summary>
        /// Opens the config window.
        /// </summary>
        internal void OnOpenConfigUi() => this.configWindow.IsOpen = true;

        private void LoadTerritories()
        {
            var sheet = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.TerritoryType>()!;
            foreach (var row in sheet)
            {
                var zone = row.PlaceName.Value;
                if (zone == null)
                    continue;

                var text = this.GetSeStringText((SeString)zone.Name);
                if (string.IsNullOrEmpty(text))
                    continue;

                this.TerritoryNames.Add(row.RowId, text);
            }
        }

        #region SeString

        private unsafe SeString GetSeString(byte* textPtr)
        {
            return this.GetSeString((IntPtr)textPtr);
        }

        private SeString GetSeString(IntPtr textPtr)
        {
            return MemoryHelper.ReadSeStringNullTerminated(textPtr);
        }

        private SeString GetSeString(byte[] bytes)
        {
            return SeString.Parse(bytes);
        }

        private string GetSeStringText(SeString sestring)
        {
            var pieces = sestring.Payloads.OfType<TextPayload>().Select(t => t.Text);
            var text = string.Join(string.Empty, pieces).Replace('\n', ' ').Trim();
            return text;
        }

        #endregion

        #region Commands

        private void OnChatCommand(string command, string arguments)
        {
            if (arguments.IsNullOrEmpty())
            {
                this.configWindow.Toggle();
                return;
            }

            switch (arguments)
            {
                case "help":
                    this.CommandHelpMenu();
                    break;
                case "last":
                    this.CommandAddNode(this.LastSeenDialogText, false, Service.Configuration.RootFolder);
                    break;
                case "last zone":
                    this.CommandAddNode(this.LastSeenDialogText, true, Service.Configuration.RootFolder);
                    break;
                default:
                    this.PrintError("I didn't quite understand that.");
                    return;
            }
        }

        private void CommandHelpMenu()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Help menu");
            sb.AppendLine($"{Command}           - Toggle the config window.");
            sb.AppendLine($"{Command} last      - Add the last seen YesNo dialog.");
            sb.AppendLine($"{Command} last zone - Add the last seen YesNo dialog with the current zone name.");
            this.PrintMessage(sb.ToString());
        }

        private void CommandAddNode(string text, bool zoneRestricted, TextFolderNode parent)
        {
            if (text.IsNullOrEmpty())
            {
                if (this.LastSeenDialogText.IsNullOrEmpty())
                {
                    this.PrintError("No dialog has been seen.");
                    return;
                }

                text = this.LastSeenDialogText;
            }

            var newNode = new TextEntryNode { Enabled = true, Text = text };

            if (zoneRestricted)
            {
                var currentID = Service.ClientState.TerritoryType;
                if (!Service.Plugin.TerritoryNames.TryGetValue(currentID, out var zoneName))
                {
                    this.PrintError("Could not find zone name.");
                    return;
                }

                newNode.ZoneRestricted = true;
                newNode.ZoneText = zoneName;
            }

            parent.Children.Add(newNode);
            Service.Configuration.Save();

            this.PrintMessage("Added a new text entry.");
        }

        #endregion
    }

    /// <summary>
    /// YesNo text matching features.
    /// </summary>
    public sealed partial class YesAlreadyPlugin
    {
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

        private unsafe void AddonSelectYesNoExecute(IntPtr addon, bool yes)
        {
            if (yes)
            {
                var addonObj = (AddonSelectYesno*)addon;
                var yesButton = addonObj->YesButton;
                if (yesButton != null && !yesButton->IsEnabled)
                {
                    PluginLog.Debug($"AddonSelectYesNo: Enabling yes button");
                    yesButton->AtkComponentBase.OwnerNode->AtkResNode.Flags ^= 1 << 5;
                }

                PluginLog.Debug($"AddonSelectYesNo: Selecting yes");
                ClickSelectYesNo.Using(addon).Yes();
            }
            else
            {
                PluginLog.Debug($"AddonSelectYesNo: Selecting no");
                ClickSelectYesNo.Using(addon).No();
            }
        }

        private IntPtr AddonSelectYesNoOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug($"AddonSelectYesNo.OnSetup");
            var result = this.addonSelectYesNoOnSetupHook.Original(addon, a2, dataPtr);

            this.SafelyNow(() =>
            {
                var data = Marshal.PtrToStructure<AddonSelectYesNoOnSetupData>(dataPtr);
                var text = this.LastSeenDialogText = this.GetSeStringText(this.GetSeString(data.TextPtr));

                PluginLog.Debug($"AddonSelectYesNo: text={text}");

                var zoneWarnOnce = true;
                var nodes = Service.Configuration.GetAllNodes().OfType<TextEntryNode>();
                foreach (var node in nodes)
                {
                    if (!node.Enabled || string.IsNullOrEmpty(node.Text))
                        continue;

                    if (!this.EntryMatchesText(node, text))
                        continue;

                    if (node.ZoneRestricted && !string.IsNullOrEmpty(node.ZoneText))
                    {
                        if (!this.TerritoryNames.TryGetValue(Service.ClientState.TerritoryType, out var zoneName))
                        {
                            if (zoneWarnOnce && !(zoneWarnOnce = false))
                            {
                                PluginLog.Debug("Unable to verify Zone Restricted entry, ZoneID was not set yet");
                                this.PrintMessage($"Unable to verify Zone Restricted entry, change zones to update value");
                            }

                            zoneName = string.Empty;
                        }

                        if (!string.IsNullOrEmpty(zoneName) && this.EntryMatchesZoneName(node, zoneName))
                        {
                            PluginLog.Debug($"AddonSelectYesNo: Matched on {node.Text} ({node.ZoneText})");
                            this.AddonSelectYesNoExecute(addon, node.IsYes);
                            break;
                        }
                    }
                    else
                    {
                        PluginLog.Debug($"AddonSelectYesNo: Matched on {node.Text}");
                        this.AddonSelectYesNoExecute(addon, node.IsYes);
                        break;
                    }
                }
            });

            return result;
        }

        [StructLayout(LayoutKind.Explicit, Size = 0x10)]
        private struct AddonSelectYesNoOnSetupData
        {
            [FieldOffset(0x8)]
            public IntPtr TextPtr;
        }
    }

    /// <summary>
    /// Non text matching features.
    /// </summary>
    public sealed partial class YesAlreadyPlugin
    {
        private int itemInspectionCount = 0;

        private void SafelyNow(Action action)
        {
            if (!Service.Configuration.Enabled)
                return;

            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Don't crash the game");
            }
        }

        private unsafe IntPtr AddonSalvageDialogOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug("AddonSalvageDialog.OnSetup");
            var result = this.addonSalvageDialogOnSetupHook.Original(addon, a2, dataPtr);

            this.SafelyNow(() =>
            {
                if (Service.Configuration.DesynthBulkDialogEnabled)
                {
                    ((AddonSalvageDialog*)addon)->BulkDesynthEnabled = true;
                }

                if (Service.Configuration.DesynthDialogEnabled)
                {
                    var clickAddon = ClickSalvageDialog.Using(addon);
                    clickAddon.CheckBox();
                    clickAddon.Desynthesize();
                }
            });

            return result;
        }

        private IntPtr AddonMaterializeDialogOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug("AddonMaterializeDialog.OnSetupDetour");
            var result = this.addonMaterializeDialogOnSetupHook.Original(addon, a2, dataPtr);

            this.SafelyNow(() =>
            {
                if (!Service.Configuration.MaterializeDialogEnabled)
                    return;

                ClickMaterializeDialog.Using(addon).Materialize();
            });

            return result;
        }

        private IntPtr AddonMateriaRetrieveDialogOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug("AddonMateriaRetrieveDialog.OnSetupDetour");
            var result = this.addonMateriaRetrieveDialogOnSetupHook.Original(addon, a2, dataPtr);

            this.SafelyNow(() =>
            {
                if (!Service.Configuration.MateriaRetrieveDialogEnabled)
                    return;

                ClickMateriaRetrieveDialog.Using(addon).Begin();
            });

            return result;
        }

        private unsafe IntPtr AddonItemInspectionResultOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug("AddonItemInspectionResult.OnSetup");
            var result = this.addonItemInspectionResultOnSetupHook.Original(addon, a2, dataPtr);

            this.SafelyNow(() =>
            {
                if (!Service.Configuration.ItemInspectionResultEnabled)
                    return;

                var addonPtr = (AddonItemInspectionResult*)addon;
                if (addonPtr->AtkUnitBase.UldManager.NodeListCount < 64)
                    return;

                var nameNode = (AtkTextNode*)addonPtr->AtkUnitBase.UldManager.NodeList[64];
                var descNode = (AtkTextNode*)addonPtr->AtkUnitBase.UldManager.NodeList[55];
                if (!nameNode->AtkResNode.IsVisible || !descNode->AtkResNode.IsVisible)
                    return;

                var nameText = this.GetSeString(nameNode->NodeText.StringPtr);
                var descText = this.GetSeStringText(this.GetSeString(descNode->NodeText.StringPtr));
                // This is hackish, but works well enough (for now).
                // Languages that dont contain the magic character will need special handling.
                if (descText.Contains("※") || descText.Contains("liées à Garde-la-Reine"))
                {
                    nameText.Payloads.Insert(0, new TextPayload("Received: "));
                    this.PrintMessage(nameText);
                }

                this.itemInspectionCount++;
                var rateLimiter = Service.Configuration.ItemInspectionResultRateLimiter;
                if (rateLimiter != 0 && this.itemInspectionCount % rateLimiter == 0)
                {
                    this.itemInspectionCount = 0;
                    this.PrintMessage("Rate limited, pausing item inspection loop.");
                    return;
                }

                ClickItemInspectionResult.Using(addon).Next();
            });

            return result;
        }

        private IntPtr AddonRetainerTaskAskOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug("AddonRetainerTaskAsk.OnSetup");
            var result = this.addonRetainerTaskAskOnSetupHook.Original(addon, a2, dataPtr);

            this.SafelyNow(() =>
            {
                if (!Service.Configuration.RetainerTaskAskEnabled)
                    return;

                ClickRetainerTaskAsk.Using(addon).Assign();
            });

            return result;
        }

        private IntPtr AddonRetainerTaskResultOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug("AddonRetainerTaskResult.OnSetup");
            var result = this.addonRetainerTaskResultOnSetupHook.Original(addon, a2, dataPtr);

            this.SafelyNow(() =>
            {
                if (!Service.Configuration.RetainerTaskResultEnabled)
                    return;

                ClickRetainerTaskResult.Using(addon).Reassign();
            });

            return result;
        }

        private IntPtr AddonGrandCompanySupplyRewardOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug("AddonGrandCompanySupplyReward.OnSetup");
            var result = this.addonGrandCompanySupplyRewardOnSetupHook.Original(addon, a2, dataPtr);

            this.SafelyNow(() =>
            {
                if (!Service.Configuration.GrandCompanySupplyReward)
                    return;

                ClickGrandCompanySupplyReward.Using(addon).Deliver();
            });

            return result;
        }

        private IntPtr AddonShopCardDialogOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug("AddonShopCardDialog.OnSetup");
            var result = this.addonShopCardDialogOnSetupHook.Original(addon, a2, dataPtr);

            this.SafelyNow(() =>
            {
                if (!Service.Configuration.ShopCardDialog)
                    return;

                ClickShopCardDialog.Using(addon).Sell();
            });

            return result;
        }

        private unsafe IntPtr AddonJournalResultOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug("AddonJournalResultComplete.OnSetup");
            var result = this.addonJournalResultOnSetupHook.Original(addon, a2, dataPtr);

            this.SafelyNow(() =>
            {
                if (!Service.Configuration.JournalResultCompleteEnabled)
                    return;

                var addonPtr = (AddonJournalResult*)addon;
                var completeButton = addonPtr->CompleteButton;
                if (!addonPtr->CompleteButton->IsEnabled)
                    return;

                ClickJournalResult.Using(addon).Complete();
            });

            return result;
        }

        private IntPtr AddonContentsFinderConfirmOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug("AddonContentsFinderConfirm.OnSetup");
            var result = this.addonContentsFinderConfirmOnSetupHook.Original(addon, a2, dataPtr);

            this.SafelyNow(() =>
            {
                if (!Service.Configuration.ContentsFinderConfirmEnabled)
                    return;

                ClickContentsFinderConfirm.Using(addon).Commence();
            });

            return result;
        }
    }
}
