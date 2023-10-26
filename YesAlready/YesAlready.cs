using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ECommons;
using ECommons.DalamudServices;
using YesAlready.UI;
using System.Collections.Generic;
using System;
using Dalamud.Game.ClientState.Keys;
using System.Text;
using YesAlready.Interface;
using Dalamud.Game.Text.SeStringHandling;
using ClickLib;
using YesAlready.BaseFeatures;
using System.Reflection;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.IoC;
using Dalamud.Plugin.Services;

namespace YesAlready;

public class YesAlready : IDalamudPlugin
{
    [PluginService] public static IAddonLifecycle AddonLifecycle { get; private set; }

    public static string Name => "YesAlready";
    private const string Command = "/yesalready";
    private static string[] Aliases => new string[] { "/pyes" };
    private readonly List<string> registeredCommands = new();
    internal Configuration Configuration { get; init; }
    internal WindowSystem Ws;
    internal MainWindow MainWindow;
    internal Configuration Config;
    private readonly ZoneListWindow zoneListWindow;

    internal static YesAlready P;
    internal static DalamudPluginInterface pi;

    private DtrBarEntry dtrEntry;

    public YesAlready(DalamudPluginInterface pluginInterface)
    {
        P = this;
        pi = pluginInterface;
        ECommonsMain.Init(pi, P);
        Ws = new();
        MainWindow = new();
        Ws.AddWindow(MainWindow);

        Config = pi.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(Svc.PluginInterface);

        Svc.Commands.AddHandler(Command, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens the plugin window.",
            ShowInHelp = true
        });
        registeredCommands.Add(Command);

        foreach (var a in Aliases)
        {
            if (!Svc.Commands.Commands.ContainsKey(a))
            {
                Svc.Commands.AddHandler(a, new CommandInfo(OnCommand)
                {
                    HelpMessage = $"{Command} Alias",
                    ShowInHelp = true
                });
                registeredCommands.Add(a);
            }
        }

        Click.Initialize();
        LoadTerritories();
        EnableFeatures(true);

        Svc.Framework.Update += FrameworkUpdate;
        Svc.PluginInterface.UiBuilder.Draw += Ws.Draw;
        Svc.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
    }

    public static void EnableFeatures(bool enable)
    {
        var featureAssembly = Assembly.GetExecutingAssembly();

        foreach (var type in featureAssembly.GetTypes())
        {
            if (typeof(BaseFeature).IsAssignableFrom(type) && !type.IsAbstract)
            {
                var feature = (BaseFeature)Activator.CreateInstance(type);
                if (enable)
                    feature.Enable();
                else
                    feature.Disable();
            }
        }
    }

    public void Dispose()
    {
        foreach (var c in registeredCommands)
        {
            Svc.Commands.RemoveHandler(c);
        }
        registeredCommands.Clear();

        Svc.Framework.Update -= FrameworkUpdate;

        Svc.PluginInterface.UiBuilder.Draw -= Ws.Draw;
        Svc.PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;

        Ws.RemoveAllWindows();
        MainWindow = null;
        Ws = null;
        ECommonsMain.Dispose();
        pi = null;
        P = null;
    }

    public void DrawConfigUI() => MainWindow.IsOpen = !MainWindow.IsOpen;
    internal void OpenZoneListUi() => zoneListWindow.IsOpen = true;

    internal Dictionary<uint, string> TerritoryNames { get; } = new();
    internal string LastSeenDialogText { get; set; } = string.Empty;
    internal string LastSeenListSelection { get; set; } = string.Empty;
    internal string LastSeenListTarget { get; set; } = string.Empty;
    internal string LastSeenTalkTarget { get; set; } = string.Empty;
    internal DateTime EscapeLastPressed { get; private set; } = DateTime.MinValue;
    internal string EscapeTargetName { get; private set; } = string.Empty;
    internal bool ForcedYesKeyPressed { get; private set; } = false;
    internal bool DisableKeyPressed { get; private set; } = false;
    internal ListEntryNode LastSelectedListNode { get; set; } = new();

    private void LoadTerritories()
    {
        var sheet = Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.TerritoryType>()!;
        foreach (var row in sheet)
        {
            var zone = row.PlaceName.Value;
            if (zone == null)
                continue;

            var text = Utils.SEString.GetSeStringText((SeString)zone.Name);
            if (string.IsNullOrEmpty(text))
                continue;

            TerritoryNames.Add(row.RowId, text);
        }
    }

    private void FrameworkUpdate(object framework)
    {
        if (Config.DisableKey != VirtualKey.NO_KEY)
        {
            DisableKeyPressed = Svc.KeyState[Config.DisableKey];
        }
        else
        {
            DisableKeyPressed = false;
        }

        if (Config.ForcedYesKey != VirtualKey.NO_KEY)
        {
            ForcedYesKeyPressed = Svc.KeyState[Config.ForcedYesKey];
        }
        else
        {
            ForcedYesKeyPressed = false;
        }

        if (Svc.KeyState[VirtualKey.ESCAPE])
        {
            EscapeLastPressed = DateTime.Now;

            var target = Svc.Targets.Target;
            EscapeTargetName = target != null
                ? Utils.SEString.GetSeStringText(target.Name)
                : string.Empty;
        }

        if (P.Config.DTRSupport)
        {
            try
            {
                dtrEntry ??= Svc.DtrBar.Get("YesAlready");
            }
            catch
            {
                return;
            }

            if (!dtrEntry.Shown) dtrEntry.Shown = true;

            dtrEntry.Text = new SeString(
                    new TextPayload($"YesAlready: {(P.Config.Enabled ? "On" : "Off")}"));

            dtrEntry.OnClick = () => P.Config.Enabled ^= true;
        }
        else if (dtrEntry != null && dtrEntry.Shown)
        {
            dtrEntry.Shown = false;
        }
    }

    #region Commands

    private void OnCommand(string command, string arguments)
    {
        if (arguments.IsNullOrEmpty())
        {
            MainWindow.IsOpen = !MainWindow.IsOpen;
            return;
        }

        switch (arguments)
        {
            case "help":
                CommandHelpMenu();
                break;
            case "toggle":
                Config.Enabled ^= true;
                Config.Save();
                break;
            case "last":
                CommandAddNode(false, false, false);
                break;
            case "last no":
                CommandAddNode(false, false, true);
                break;
            case "last zone":
                CommandAddNode(true, false, false);
                break;
            case "last zone no":
                CommandAddNode(true, false, true);
                break;
            case "last zone folder":
                CommandAddNode(true, true, false);
                break;
            case "last zone folder no":
                CommandAddNode(true, true, true);
                break;
            case "lastlist":
                CommandAddListNode();
                break;
            case "lasttalk":
                CommandAddTalkNode();
                break;
            case "dutyconfirm":
                ToggleDutyConfirm();
                break;
            case "onetimeconfirm":
                ToggleOneTimeConfirm();
                break;
            default:
                Svc.Log.Error("I didn't quite understand that.");
                return;
        }
    }

    private static void CommandHelpMenu()
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
        Svc.Log.Info(sb.ToString());
    }

    private void CommandAddNode(bool zoneRestricted, bool createFolder, bool selectNo)
    {
        var text = LastSeenDialogText;

        if (text.IsNullOrEmpty())
        {
            Svc.Log.Error("No dialog has been seen.");
            return;
        }

        Configuration.CreateTextNode(Config.RootFolder, zoneRestricted, createFolder, selectNo);
        Config.Save();

        Svc.Log.Info("Added a new text entry.");
    }

    private void CommandAddListNode()
    {
        var text = LastSeenListSelection;
        var target = LastSeenListTarget;

        if (text.IsNullOrEmpty())
        {
            Svc.Log.Error("No dialog has been selected.");
            return;
        }

        var newNode = new ListEntryNode { Enabled = true, Text = text };

        if (!target.IsNullOrEmpty())
        {
            newNode.TargetRestricted = true;
            newNode.TargetText = target;
        }

        var parent = Config.ListRootFolder;
        parent.Children.Add(newNode);
        Config.Save();

        Svc.Log.Info("Added a new list entry.");
    }

    private void CommandAddTalkNode()
    {
        var target = LastSeenTalkTarget;

        if (target.IsNullOrEmpty())
        {
            Svc.Log.Error("No talk dialog has been seen.");
            return;
        }

        var newNode = new TalkEntryNode { Enabled = true, TargetText = target };

        var parent = Config.TalkRootFolder;
        parent.Children.Add(newNode);
        Config.Save();

        Svc.Log.Info("Added a new talk entry.");
    }

    private void ToggleDutyConfirm()
    {
        Config.ContentsFinderConfirmEnabled ^= true;
        Config.ContentsFinderOneTimeConfirmEnabled = false;
        Config.Save();

        var state = Config.ContentsFinderConfirmEnabled ? "enabled" : "disabled";
        Svc.Log.Info($"Duty Confirm {state}.");
    }

    private void ToggleOneTimeConfirm()
    {
        Config.ContentsFinderOneTimeConfirmEnabled ^= true;
        Config.ContentsFinderConfirmEnabled = Config.ContentsFinderOneTimeConfirmEnabled;
        Config.Save();

        var state = Config.ContentsFinderOneTimeConfirmEnabled ? "enabled" : "disabled";
        Svc.Log.Info($"Duty Confirm and One Time Confirm {state}.");
    }

    #endregion
}

