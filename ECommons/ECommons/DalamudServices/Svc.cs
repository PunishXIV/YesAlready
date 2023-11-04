using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Buddy;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Game.ClientState.GamePad;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Command;
using Dalamud.Game.Config;
using Dalamud.Game.DutyState;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Gui.PartyFinder;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Libc;
using Dalamud.Game.Network;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ECommons.Logging;
using System;

namespace ECommons.DalamudServices;

//If one of services is not ready, whole service class will be unavailable.
//This is inconvenient. Let's bypass it.
public class Svc
{
    [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; }
    [PluginService] public static IBuddyList Buddies { get; private set; }
    [PluginService] public static IChatGui Chat { get; private set; }
    [PluginService] public static IClientState ClientState { get; private set; }
    [PluginService] public static ICommandManager Commands { get; private set; }
    [PluginService] public static ICondition Condition { get; private set; }
    [PluginService] public static IDataManager Data { get; private set; }
    [PluginService] public static IFateTable Fates { get; private set; }
    [PluginService] public static IFlyTextGui FlyText { get; private set; }
    [PluginService] public static IFramework Framework { get; private set; }
    [PluginService] public static IGameGui GameGui { get; private set; }
    [PluginService] public static IGameNetwork GameNetwork { get; private set; }
    [PluginService] public static IJobGauges Gauges { get; private set; }
    [PluginService] public static IKeyState KeyState { get; private set; }
    [PluginService] public static ILibcFunction LibcFunction { get; private set; }
    [PluginService] public static IObjectTable Objects { get; private set; }
    [PluginService] public static IPartyFinderGui PfGui { get; private set; }
    [PluginService] public static IPartyList Party { get; private set; }
    [PluginService] public static ISigScanner SigScanner { get; private set; }
    [PluginService] public static ITargetManager Targets { get; private set; }
    [PluginService] public static IToastGui Toasts { get; private set; }
    [PluginService] public static IGameConfig GameConfig { get; private set; }
    [PluginService] public static IGameLifecycle GameLifecycle { get; private set; }
    [PluginService] public static IGamepadState GamepadState { get; private set; }
    [PluginService] public static IDtrBar DtrBar { get; private set; }
    [PluginService] public static IDutyState DutyState { get; private set; }
    [PluginService] public static IGameInteropProvider Hook { get; private set; }
    [PluginService] public static ITextureProvider Texture { get; private set; }
    [PluginService] public static IPluginLog Log { get; private set; }

    internal static bool IsInitialized = false;
    public static void Init(DalamudPluginInterface pi)
    {
        if (IsInitialized)
        {
            PluginLog.Debug("Services already initialized, skipping");
        }
        IsInitialized = true;
        try
        {
            pi.Create<Svc>();
        }
        catch(Exception ex)
        {
            ex.Log();
        }
    }
}