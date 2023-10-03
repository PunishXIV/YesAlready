using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace YesAlready;

/// <summary>
/// Dalamud and plugin services.
/// </summary>
internal class Service
{
    /// <summary>
    /// Gets or sets the plugin itself.
    /// </summary>
    internal static YesAlreadyPlugin Plugin { get; set; } = null!;

    /// <summary>
    /// Gets or sets the plugin configuration.
    /// </summary>
    internal static YesAlreadyConfiguration Configuration { get; set; } = null!;

    /// <summary>
    /// Gets the Dalamud plugin interface.
    /// </summary>
    [PluginService]
    internal static DalamudPluginInterface Interface { get; private set; } = null!;

    /// <summary>
    /// Gets the Dalamud chat gui.
    /// </summary>
    [PluginService]
    internal static IChatGui ChatGui { get; private set; } = null!;

    /// <summary>
    /// Gets the Dalamud client state.
    /// </summary>
    [PluginService]
    internal static IClientState ClientState { get; private set; } = null!;

    /// <summary>
    /// Gets the Dalamud command manager.
    /// </summary>
    [PluginService]
    internal static ICommandManager CommandManager { get; private set; } = null!;

    /// <summary>
    /// Gets the Dalamud data manager.
    /// </summary>
    [PluginService]
    internal static IDataManager DataManager { get; private set; } = null!;

    /// <summary>
    /// Gets the Dalamud framework.
    /// </summary>
    [PluginService]
    internal static IFramework Framework { get; private set; } = null!;

    /// <summary>
    /// Gets the Dalamud game gui.
    /// </summary>
    [PluginService]
    internal static IGameGui GameGui { get; private set; } = null!;

    /// <summary>
    /// Gets the Dalamud signature scanner.
    /// </summary>
    [PluginService]
    internal static ISigScanner Scanner { get; private set; } = null!;

    /// <summary>
    /// Gets the Dalamud keystate manager.
    /// </summary>
    [PluginService]
    internal static IKeyState KeyState { get; private set; } = null!;

    /// <summary>
    /// Gets the Dalamud target manager.
    /// </summary>
    [PluginService]
    internal static ITargetManager TargetManager { get; private set; } = null!;
    [PluginService]
    internal static IGameInteropProvider Hook { get; private set; } = null!;
}
