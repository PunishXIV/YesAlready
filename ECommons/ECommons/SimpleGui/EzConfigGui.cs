using Dalamud.Configuration;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using ECommons.Reflection;
using System;

namespace ECommons.SimpleGui;

public static class EzConfigGui
{
    public static WindowSystem WindowSystem { get; internal set; }
    internal static Action Draw = null;
    internal static Action OnClose = null;
    internal static Action OnOpen = null;
    internal static IPluginConfiguration Config;
    static ConfigWindow configWindow;
    static string Ver = string.Empty;
    public static Window Window { get { return configWindow; } }

    public static void Init(Action draw, IPluginConfiguration config = null)
    {
        if(WindowSystem != null)
        {
            throw new Exception("ConfigGui already initialized");
        }
        WindowSystem = new($"ECommons@{DalamudReflector.GetPluginName()}");
        Draw = draw;
        Config = config;
        Ver = ECommonsMain.Instance.GetType().Assembly.GetName().Version.ToString();
        configWindow = new($"{DalamudReflector.GetPluginName()} v{Ver}###{DalamudReflector.GetPluginName()}");
        WindowSystem.AddWindow(configWindow);
        Svc.PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        Svc.PluginInterface.UiBuilder.OpenConfigUi += Open;
    }

    public static void Open()
    {
        configWindow.IsOpen = true;
    }
    
    public static void Open(string cmd = null, string args = null)
    {
        Open();
    }
}
