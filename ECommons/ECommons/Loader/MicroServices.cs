using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;

namespace ECommons.Loader
{
    internal class MicroServices
    {
        [PluginService] static internal DalamudPluginInterface PluginInterface { get; private set; }
        //[PluginService] static internal BuddyList Buddies { get; private set; }
        //[PluginService] static internal ChatGui Chat { get; private set; }
        //[PluginService] static internal ChatHandlers ChatHandlers { get; private set; }
        //[PluginService] static internal ClientState ClientState { get; private set; }
        [PluginService] static internal ICommandManager Commands { get; private set; }
        //[PluginService] static internal Condition Condition { get; private set; }
        //[PluginService] static internal DataManager Data { get; private set; }
        //[PluginService] static internal FateTable Fates { get; private set; }
        //[PluginService] static internal FlyTextGui FlyText { get; private set; }
        [PluginService] static internal IFramework Framework { get; private set; }
        //[PluginService] static internal GameGui GameGui { get; private set; }
        //[PluginService] static internal GameNetwork GameNetwork { get; private set; }
        //[PluginService] static internal JobGauges Gauges { get; private set; }
        //[PluginService] static internal KeyState KeyState { get; private set; }
        //[PluginService] static internal LibcFunction LibcFunction { get; private set; }
        //[PluginService] static internal ObjectTable Objects { get; private set; }
        //[PluginService] static internal PartyFinderGui PfGui { get; private set; }
        //[PluginService] static internal PartyList Party { get; private set; }
        //[PluginService] static internal SeStringManager SeStringManager { get; private set; }
        //[PluginService] static internal SigScanner SigScanner { get; private set; }
        //[PluginService] static internal TargetManager Targets { get; private set; }
        //[PluginService] static internal ToastGui Toasts { get; private set; }
    }
}
