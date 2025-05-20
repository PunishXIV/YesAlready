using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ECommons.EzDTR;
using ECommons.EzHookManager;
using ECommons.SimpleGui;
using ECommons.Singletons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using YesAlready.Interface;
using YesAlready.UI;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace YesAlready;

public class YesAlready : IDalamudPlugin
{
    public static string Name => "YesAlready";
    public static YesAlready P { get; private set; } = null!;
    public static Configuration C { get; set; } = null!;

    private const string Command = "/yesalready";
    private readonly string[] Aliases = ["/pyes"];

    private unsafe delegate void* FireCallbackDelegate(AtkUnitBase* atkUnitBase, int valueCount, AtkValue* atkValues, byte updateVisibility);
    [EzHook("E8 ?? ?? ?? ?? 0F B6 E8 8B 44 24 20", detourName: nameof(FireCallbackDetour), true)]
    private readonly EzHook<FireCallbackDelegate> FireCallbackHook = null!;

    internal bool Active => C.Enabled && !Service.BlockListHandler.Locked;

    public YesAlready(IDalamudPluginInterface pluginInterface)
    {
        P = this;
        ECommonsMain.Init(pluginInterface, P);
        SingletonServiceManager.Initialize(typeof(Service));

        C = Svc.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        C.Migrate();

        EzConfigGui.Init(new MainWindow().Draw);
        EzConfigGui.WindowSystem.AddWindow(new ZoneListWindow());
        EzConfigGui.WindowSystem.AddWindow(new ConditionsListWindow());

        EzCmd.Add(Command, OnCommand, "Opens the plugin window.", int.MinValue);
        Aliases.Each(a => EzCmd.Add(a, OnCommand, $"{Command} alias"));

        _ = new EzDtr(() => new SeString(new TextPayload($"{Name}: {(C.Enabled ? (Service.BlockListHandler.Locked ? "Paused" : "On") : "Off")}")), () => C.Enabled ^= true);

        LoadTerritories();
        ToggleFeatures(true);

        Svc.Framework.Update += FrameworkUpdate;
        Svc.PluginInterface.UiBuilder.OpenMainUi += DrawConfigUI;

        EzSignatureHelper.Initialize(this);
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
        var featureAssembly = Assembly.GetExecutingAssembly();
        var type = typeof(T);

        if (!typeof(BaseFeature).IsAssignableFrom(type) || type.IsAbstract)
            return null;

        if (Activator.CreateInstance(type) is T feature)
            return feature;

        return null;
    }

    public void Dispose()
    {
        Svc.Framework.Update -= FrameworkUpdate;
        Svc.PluginInterface.UiBuilder.OpenMainUi -= DrawConfigUI;
        ECommonsMain.Dispose();
    }

    public void DrawConfigUI() => EzConfigGui.Window.IsOpen ^= true;
    internal void OpenZoneListUi() => EzConfigGui.WindowSystem.Windows.First(w => w.WindowName == ZoneListWindow.Title).IsOpen ^= true;
    internal void OpenConditionsListUi() => EzConfigGui.WindowSystem.Windows.First(w => w.WindowName == ConditionsListWindow.Title).IsOpen ^= true;

    internal Dictionary<uint, string> TerritoryNames { get; private set; } = [];
    internal string LastSeenDialogText { get; set; } = string.Empty;
    internal string LastSeenOkText { get; set; } = string.Empty;
    internal string LastSeenListSelection { get; set; } = string.Empty;
    internal int LastSeenListIndex { get; set; }
    internal string LastSeenListTarget { get; set; } = string.Empty;
    internal (int Index, string Text)[] LastSeenListEntries { get; set; } = [];
    internal string LastSeenTalkTarget { get; set; } = string.Empty;
    internal string LastSeenNumericsText { get; set; } = string.Empty;
    internal DateTime EscapeLastPressed { get; private set; } = DateTime.MinValue;
    internal string EscapeTargetName { get; private set; } = string.Empty;
    internal bool ForcedYesKeyPressed { get; private set; } = false;
    internal bool ForcedTalkKeyPressed { get; private set; } = false;
    internal bool DisableKeyPressed { get; private set; } = false;
    internal ListEntryNode LastSelectedListNode { get; set; } = new();

    private void LoadTerritories()
        => TerritoryNames = GenericHelpers.FindRows<Lumina.Excel.Sheets.TerritoryType>(r => r.PlaceName.IsValid && !r.PlaceName.Value.Name.IsEmpty)
            .Select((r, n) => (r.RowId, PlaceName: r.PlaceName.Value.Name.ToString())).ToDictionary(t => t.RowId, t => t.PlaceName);

    private bool wasDisableKeyPressed;
    private void FrameworkUpdate(IFramework framework)
    {
        if (!P.Active && !wasDisableKeyPressed) return;
        DisableKeyPressed = C.DisableKey != VirtualKey.NO_KEY && Svc.KeyState[C.DisableKey];

        if (P.Active && DisableKeyPressed && !wasDisableKeyPressed)
            C.Enabled = false;
        else if (!P.Active && !DisableKeyPressed && wasDisableKeyPressed)
            C.Enabled = true;

        wasDisableKeyPressed = DisableKeyPressed;

        ForcedYesKeyPressed = C.ForcedYesKey != VirtualKey.NO_KEY && Svc.KeyState[C.ForcedYesKey];

        ForcedTalkKeyPressed = C.ForcedTalkKey != VirtualKey.NO_KEY && C.SeparateForcedKeys && Svc.KeyState[C.ForcedTalkKey];

        if (Svc.KeyState[VirtualKey.ESCAPE])
        {
            EscapeLastPressed = DateTime.Now;

            var target = Svc.Targets.Target;
            EscapeTargetName = target != null ? target.Name.GetText() : string.Empty;
        }
    }

    private unsafe void* FireCallbackDetour(AtkUnitBase* atkUnitBase, int valueCount, AtkValue* atkValues, byte updateVisibility)
    {
        if (atkUnitBase->NameString is not ("SelectString" or "SelectIconString"))
            return FireCallbackHook.Original(atkUnitBase, valueCount, atkValues, updateVisibility);

        try
        {
            var atkValueList = Enumerable.Range(0, valueCount)
                .Select<int, object>(i => atkValues[i].Type switch
                {
                    ValueType.Int => atkValues[i].Int,
                    ValueType.String => Marshal.PtrToStringUTF8(new IntPtr(atkValues[i].String)) ?? string.Empty,
                    ValueType.UInt => atkValues[i].UInt,
                    ValueType.Bool => atkValues[i].Byte != 0,
                    _ => $"Unknown Type: {atkValues[i].Type}"
                })
                .ToList();
            PluginLog.Debug($"Callback triggered on {atkUnitBase->NameString} with values: {string.Join(", ", atkValueList.Select(value => value.ToString()))}");
            LastSeenListIndex = atkValues[0].Int;
        }
        catch (Exception ex)
        {
            PluginLog.Error($"Exception in {nameof(FireCallbackDetour)}: {ex.Message}");
            return FireCallbackHook.Original(atkUnitBase, valueCount, atkValues, updateVisibility);
        }
        return FireCallbackHook.Original(atkUnitBase, valueCount, atkValues, updateVisibility);
    }

    #region Commands

    private void OnCommand(string command, string arguments)
    {
        if (arguments.IsNullOrEmpty())
        {
            EzConfigGui.Window.IsOpen ^= true;
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
        Utils.SEString.PrintPluginMessage(sb.ToString());
    }

    private void CommandAddNode(bool zoneRestricted, bool createFolder, bool selectNo)
    {
        var text = LastSeenDialogText;

        if (text.IsNullOrEmpty())
        {
            PluginLog.Error("No dialog has been seen.");
            return;
        }

        Configuration.CreateTextNode(C.RootFolder, zoneRestricted, createFolder, selectNo);
        C.Save();

        Utils.SEString.PrintPluginMessage("Added a new text entry.");
    }

    private void CommandAddOkNode(bool createFolder)
    {
        var text = LastSeenOkText;

        if (text.IsNullOrEmpty())
        {
            PluginLog.Error("No dialog has been seen.");
            return;
        }

        Configuration.CreateOkNode(C.RootFolder, createFolder);
        C.Save();

        Utils.SEString.PrintPluginMessage("Added a new text entry.");
    }

    private void CommandAddListNode()
    {
        var text = LastSeenListSelection;
        var target = LastSeenListTarget;

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

        Utils.SEString.PrintPluginMessage("Added a new list entry.");
    }

    private void CommandAddTalkNode()
    {
        var target = LastSeenTalkTarget;

        if (target.IsNullOrEmpty())
        {
            PluginLog.Error("No talk dialog has been seen.");
            return;
        }

        var newNode = new TalkEntryNode { Enabled = true, TargetText = target };

        var parent = C.TalkRootFolder;
        parent.Children.Add(newNode);
        C.Save();

        Utils.SEString.PrintPluginMessage("Added a new talk entry.");
    }

    private void ToggleDutyConfirm()
    {
        C.ContentsFinderConfirmEnabled ^= true;
        C.ContentsFinderOneTimeConfirmEnabled = false;
        C.Save();

        var state = C.ContentsFinderConfirmEnabled ? "enabled" : "disabled";
        Utils.SEString.PrintPluginMessage($"Duty Confirm {state}.");
    }

    private void ToggleOneTimeConfirm()
    {
        C.ContentsFinderOneTimeConfirmEnabled ^= true;
        C.ContentsFinderConfirmEnabled = C.ContentsFinderOneTimeConfirmEnabled;
        C.Save();

        var state = C.ContentsFinderOneTimeConfirmEnabled ? "enabled" : "disabled";
        Utils.SEString.PrintPluginMessage($"Duty Confirm and One Time Confirm {state}.");
    }

    #endregion
}

