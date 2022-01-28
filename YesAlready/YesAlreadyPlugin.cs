using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ClickLib;
using Dalamud.Game;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Command;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using Dalamud.Memory;
using Dalamud.Plugin;
using Dalamud.Utility;
using YesAlready.BaseFeatures;
using YesAlready.Features;
using YesAlready.Interface;

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
        private readonly List<IBaseFeature> features = new();

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

            this.LoadTerritories();
            Click.Initialize();

            Service.Address = new PluginAddressResolver();
            Service.Address.Setup();

            Service.Framework.Update += this.FrameworkUpdate;

            this.features.Add(new AddonSelectYesNoFeature());
            this.features.Add(new AddonSelectStringFeature());
            this.features.Add(new AddonSelectIconStringFeature());
            this.features.Add(new AddonSalvageDialogFeature());
            this.features.Add(new AddonMaterializeDialogFeature());
            this.features.Add(new AddonMateriaRetrieveDialogFeature());
            this.features.Add(new AddonItemInspectionResultFeature());
            this.features.Add(new AddonRetainerTaskAskFeature());
            this.features.Add(new AddonRetainerTaskResultFeature());
            this.features.Add(new AddonGrandCompanySupplyRewardFeature());
            this.features.Add(new AddonShopCardDialogFeature());
            this.features.Add(new AddonJournalResultFeature());
            this.features.Add(new AddonContentsFinderConfirmFeature());
            this.features.Add(new AddonTalkFeature());

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

        /// <summary>
        /// Gets or sets the last selection of a list dialog.
        /// </summary>
        internal string LastSeenListSelection { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the target selected when a selection was last made in a list dialog.
        /// </summary>
        internal string LastSeenListTarget { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the target selected when a talk dialog was last updated.
        /// </summary>
        internal string LastSeenTalkTarget { get; set; } = string.Empty;

        /// <summary>
        /// Gets the datetime when the escape button was last pressed.
        /// </summary>
        internal DateTime EscapeLastPressed { get; private set; } = DateTime.MinValue;

        /// <summary>
        /// Gets a value indicating whether the forced yes hotkey is pressed.
        /// </summary>
        internal bool ForcedYesKeyPressed { get; private set; } = false;

        /// <summary>
        /// Gets a value indicating whether the disable hotkey is pressed.
        /// </summary>
        internal bool DisableKeyPressed { get; private set; } = false;

        /// <summary>
        /// Gets or sets the last selected list node, so the escape only skips that specific one.
        /// </summary>
        internal ListEntryNode LastSelectedListNode { get; set; } = new();

        /// <inheritdoc/>
        public void Dispose()
        {
            Service.Framework.Update -= this.FrameworkUpdate;

            Service.CommandManager.RemoveHandler(Command);

            Service.Interface.UiBuilder.OpenConfigUi -= this.OnOpenConfigUi;
            Service.Interface.UiBuilder.Draw -= this.windowSystem.Draw;

            this.features.ForEach(feature => feature?.Dispose());
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

        #region SeString

        /// <summary>
        /// Read an SeString.
        /// </summary>
        /// <param name="textPtr">SeString address.</param>
        /// <returns>The SeString.</returns>
        internal unsafe SeString GetSeString(byte* textPtr)
            => this.GetSeString((IntPtr)textPtr);

        /// <summary>
        /// Read an SeString.
        /// </summary>
        /// <param name="textPtr">SeString address.</param>
        /// <returns>The SeString.</returns>
        internal SeString GetSeString(IntPtr textPtr)
            => MemoryHelper.ReadSeStringNullTerminated(textPtr);

        /// <summary>
        /// Read the text of an SeString.
        /// </summary>
        /// <param name="textPtr">SeString address.</param>
        /// <returns>The SeString.</returns>
        internal unsafe string GetSeStringText(byte* textPtr)
            => this.GetSeStringText(this.GetSeString(textPtr));

        /// <summary>
        /// Read the text of an SeString.
        /// </summary>
        /// <param name="textPtr">SeString address.</param>
        /// <returns>The SeString.</returns>
        internal string GetSeStringText(IntPtr textPtr)
            => this.GetSeStringText(this.GetSeString(textPtr));

        /// <summary>
        /// Read the text of an SeString.
        /// </summary>
        /// <param name="seString">An SeString.</param>
        /// <returns>The seString.</returns>
        internal string GetSeStringText(SeString seString)
        {
            var pieces = seString.Payloads.OfType<TextPayload>().Select(t => t.Text);
            var text = string.Join(string.Empty, pieces).Replace('\n', ' ').Trim();
            return text;
        }

        #endregion

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

        private void FrameworkUpdate(Framework framework)
        {
            if (Service.Configuration.DisableKey != (int)VirtualKey.NO_KEY)
            {
                this.DisableKeyPressed = Service.KeyState[Service.Configuration.DisableKey];
            }
            else
            {
                this.DisableKeyPressed = false;
            }

            if (Service.Configuration.ForcedYesKey != (int)VirtualKey.NO_KEY)
            {
                this.ForcedYesKeyPressed = Service.KeyState[Service.Configuration.ForcedYesKey];
            }
            else
            {
                this.ForcedYesKeyPressed = false;
            }

            if (Service.KeyState[VirtualKey.ESCAPE])
                this.EscapeLastPressed = DateTime.Now;
        }

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
                case "toggle":
                    Service.Configuration.Enabled ^= true;
                    Service.Configuration.Save();
                    break;
                case "last":
                    this.CommandAddNode(false, false, false);
                    break;
                case "last no":
                    this.CommandAddNode(false, false, true);
                    break;
                case "last zone":
                    this.CommandAddNode(true, false, false);
                    break;
                case "last zone no":
                    this.CommandAddNode(true, false, true);
                    break;
                case "last zone folder":
                    this.CommandAddNode(true, true, false);
                    break;
                case "last zone folder no":
                    this.CommandAddNode(true, true, true);
                    break;
                case "lastlist":
                    this.CommandAddListNode();
                    break;
                case "lasttalk":
                    this.CommandAddTalkNode();
                    break;
                case "dutyconfirm":
                    this.ToggleDutyConfirm();
                    break;
                case "onetimeconfirm":
                    this.ToggleOneTimeConfirm();
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
            sb.AppendLine($"{Command} - Toggle the config window.");
            sb.AppendLine($"{Command} toggle - Toggle the plugin on/off.");
            sb.AppendLine($"{Command} last - Add the last seen YesNo dialog.");
            sb.AppendLine($"{Command} last no - Add the last seen YesNo dialog as a no.");
            sb.AppendLine($"{Command} last zone - Add the last seen YesNo dialog with the current zone name.");
            sb.AppendLine($"{Command} last zone no - Add the last seen YesNo dialog with the current zone name as a no.");
            sb.AppendLine($"{Command} last zone folder - Add the last seen YesNo dialog with the current zone name in a folder with the current zone name.");
            sb.AppendLine($"{Command} last zone folder no - Add the last seen YesNo dialog with the current zone name in a folder with the current zone name as a no.");
            sb.AppendLine($"{Command} lastlist - Add the last selected list dialog with the target at the time.");
            sb.AppendLine($"{Command} lasttalk - Add the last seen target during a Talk dialog.");
            sb.AppendLine($"{Command} dutyconfirm - Toggle duty confirm.");
            sb.AppendLine($"{Command} onetimeconfirm - Toggles duty confirm as well as one-time confirm.");
            this.PrintMessage(sb.ToString());
        }

        private void CommandAddNode(bool zoneRestricted, bool createFolder, bool selectNo)
        {
            var text = this.LastSeenDialogText;

            if (text.IsNullOrEmpty())
            {
                this.PrintError("No dialog has been seen.");
                return;
            }

            Service.Configuration.CreateTextNode(Service.Configuration.RootFolder, zoneRestricted, createFolder, selectNo);
            Service.Configuration.Save();

            this.PrintMessage("Added a new text entry.");
        }

        private void CommandAddListNode()
        {
            var text = this.LastSeenListSelection;
            var target = this.LastSeenListTarget;

            if (text.IsNullOrEmpty())
            {
                this.PrintError("No dialog has been selected.");
                return;
            }

            var newNode = new ListEntryNode { Enabled = true, Text = text };

            if (!target.IsNullOrEmpty())
            {
                newNode.TargetRestricted = true;
                newNode.TargetText = target;
            }

            var parent = Service.Configuration.ListRootFolder;
            parent.Children.Add(newNode);
            Service.Configuration.Save();

            this.PrintMessage("Added a new list entry.");
        }

        private void CommandAddTalkNode()
        {
            var target = this.LastSeenTalkTarget;

            if (target.IsNullOrEmpty())
            {
                this.PrintError("No talk dialog has been seen.");
                return;
            }

            var newNode = new TalkEntryNode { Enabled = true, TargetText = target };

            var parent = Service.Configuration.TalkRootFolder;
            parent.Children.Add(newNode);
            Service.Configuration.Save();

            this.PrintMessage("Added a new talk entry.");
        }

        private void ToggleDutyConfirm()
        {
            Service.Configuration.ContentsFinderConfirmEnabled ^= true;
            Service.Configuration.ContentsFinderOneTimeConfirmEnabled = false;
            Service.Configuration.Save();

            var state = Service.Configuration.ContentsFinderConfirmEnabled ? "enabled" : "disabled";
            this.PrintMessage($"Duty Confirm {state}.");
        }

        private void ToggleOneTimeConfirm()
        {
            Service.Configuration.ContentsFinderOneTimeConfirmEnabled ^= true;
            Service.Configuration.ContentsFinderConfirmEnabled = Service.Configuration.ContentsFinderOneTimeConfirmEnabled;
            Service.Configuration.Save();

            var state = Service.Configuration.ContentsFinderOneTimeConfirmEnabled ? "enabled" : "disabled";
            this.PrintMessage($"Duty Confirm and One Time Confirm {state}.");
        }

        #endregion
    }
}
