using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin;
using ECommons.EzDTR;
using ECommons.EzHookManager;
using ECommons.GameHelpers;
using ECommons.SimpleGui;
using ECommons.Singletons;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using YesAlready.Interface;
using YesAlready.UI;

namespace YesAlready;

public class YesAlready : IDalamudPlugin
{
    public static string Name => "YesAlready";
    public static YesAlready P { get; private set; } = null!;
    public static Configuration C { get; set; } = null!;

    private const string Command = "/yesalready";
    private readonly string[] Aliases = ["/pyes"];

    internal bool Active => C.Enabled && !Service.BlockListHandler.Locked;

    public YesAlready(IDalamudPluginInterface pluginInterface)
    {
        P = this;
        ECommonsMain.Init(pluginInterface, P);

        C = Svc.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        C.Migrate();

        SingletonServiceManager.Initialize(typeof(Service));
        EzConfigGui.Init(new MainWindow().Draw);
        EzConfigGui.WindowSystem.AddWindow(new ZoneListWindow());
        EzConfigGui.WindowSystem.AddWindow(new ConditionsListWindow());

        EzCmd.Add(Command, OnCommand, "Opens the plugin window.", int.MinValue);
        Aliases.Each(a => EzCmd.Add(a, OnCommand, $"{Command} alias"));

        _ = new EzDtr(() => new SeString(new TextPayload($"{Name}: {(C.Enabled ? (Service.BlockListHandler.Locked ? "Paused" : "On") : "Off")}")), () => C.Enabled ^= true);

        LoadTerritories();
        ToggleFeatures(true);

        Svc.PluginInterface.UiBuilder.OpenMainUi += EzConfigGui.Toggle;
    }

    public static void ToggleFeatures(bool enable)
    {
        var featureAssembly = Assembly.GetExecutingAssembly();

        foreach (var type in featureAssembly.GetTypes())
        {
            if (typeof(BaseFeature).IsAssignableFrom(type) && !type.IsAbstract)
            {
                if (Activator.CreateInstance(type) is BaseFeature feature)
                {
                    if (enable)
                        feature.Enable();
                    else
                        feature.Disable();
                }
            }
        }
    }

    public T? GetFeature<T>() where T : BaseFeature
    {
        var type = typeof(T);

        if (!typeof(BaseFeature).IsAssignableFrom(type) || type.IsAbstract)
            return null;

        if (Activator.CreateInstance(type) is T feature)
            return feature;

        return null;
    }

    public void Dispose()
    {
        Svc.PluginInterface.UiBuilder.OpenMainUi -= EzConfigGui.Toggle;
        ECommonsMain.Dispose();
    }

    internal Dictionary<uint, string> TerritoryNames { get; private set; } = [];

    private void LoadTerritories()
        => TerritoryNames = GenericHelpers.FindRows<TerritoryType>(r => r.PlaceName.IsValid && !r.PlaceName.Value.Name.IsEmpty)
            .Select((r, n) => (r.RowId, PlaceName: r.PlaceName.Value.Name.ToString())).ToDictionary(t => t.RowId, t => t.PlaceName);

    #region Commands

    private void OnCommand(string command, string arguments)
    {
        if (arguments.IsNullOrEmpty())
        {
            EzConfigGui.Toggle();
            return;
        }

        switch (arguments)
        {
            case "help":
                CommandHelpMenu();
                break;
            case "toggle":
                C.Enabled ^= true;
                C.Save();
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
            case "lastok":
                CommandAddOkNode(false);
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
                PluginLog.Error("I didn't quite understand that.");
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
        Svc.Chat.PrintPluginMessage(sb);
    }

    private void CommandAddNode(bool zoneRestricted, bool createFolder, bool selectNo)
    {
        var text = Service.Watcher.LastSeenDialogText;

        if (text.IsNullOrEmpty())
        {
            PluginLog.Error("No dialog has been seen.");
            return;
        }

        Configuration.CreateNode<TextEntryNode>(C.RootFolder, createFolder, zoneRestricted ? GenericHelpers.GetRow<TerritoryType>(Player.Territory)?.Name.ExtractText() : null, !selectNo);
        C.Save();

        Svc.Chat.PrintPluginMessage("Added a new text entry.");
    }

    private void CommandAddOkNode(bool createFolder)
    {
        var text = Service.Watcher.LastSeenOkText;

        if (text.IsNullOrEmpty())
        {
            PluginLog.Error("No dialog has been seen.");
            return;
        }

        Configuration.CreateNode<OkEntryNode>(C.RootFolder, createFolder);
        C.Save();

        Svc.Chat.PrintPluginMessage("Added a new text entry.");
    }

    private void CommandAddListNode()
    {
        var text = Service.Watcher.LastSeenListSelection;
        var target = Service.Watcher.LastSeenListTarget;

        if (text.IsNullOrEmpty())
        {
            PluginLog.Error("No dialog has been selected.");
            return;
        }

        var newNode = new ListEntryNode { Enabled = true, Text = text };

        if (!target.IsNullOrEmpty())
        {
            newNode.TargetRestricted = true;
            newNode.TargetText = target;
        }

        var parent = C.ListRootFolder;
        parent.Children.Add(newNode);
        C.Save();

        Svc.Chat.PrintPluginMessage("Added a new list entry.");
    }

    private void CommandAddTalkNode()
    {
        var target = Service.Watcher.LastSeenTalkTarget;

        if (target.IsNullOrEmpty())
        {
            PluginLog.Error("No talk dialog has been seen.");
            return;
        }

        var newNode = new TalkEntryNode { Enabled = true, TargetText = target };

        var parent = C.TalkRootFolder;
        parent.Children.Add(newNode);
        C.Save();

        Svc.Chat.PrintPluginMessage("Added a new talk entry.");
    }

    private void ToggleDutyConfirm()
    {
        C.ContentsFinderConfirmEnabled ^= true;
        C.ContentsFinderOneTimeConfirmEnabled = false;
        C.Save();

        var state = C.ContentsFinderConfirmEnabled ? "enabled" : "disabled";
        Svc.Chat.PrintPluginMessage($"Duty Confirm {state}.");
    }

    private void ToggleOneTimeConfirm()
    {
        C.ContentsFinderOneTimeConfirmEnabled ^= true;
        C.ContentsFinderConfirmEnabled = C.ContentsFinderOneTimeConfirmEnabled;
        C.Save();

        var state = C.ContentsFinderOneTimeConfirmEnabled ? "enabled" : "disabled";
        Svc.Chat.PrintPluginMessage($"Duty Confirm and One Time Confirm {state}.");
    }

    #endregion
}

