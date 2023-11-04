using Dalamud.Game.Command;
using ECommons.DalamudServices;
using ECommons.Reflection;
using System;
using System.Collections.Generic;
using static Dalamud.Game.Command.CommandInfo;

namespace ECommons;

public static class EzCmd
{
    internal static List<string> RegisteredCommands = new();

    //[Obsolete("Please use Cmd Attribute to the method in IDalamudPlugin to Add your command.")]
    public static void Add(string command, HandlerDelegate action, string helpMessage = null)
    {
        RegisteredCommands.Add(command);
        var cInfo = new CommandInfo(action)
        {
            HelpMessage = helpMessage ?? "",
            ShowInHelp = helpMessage != null
        };
        GenericHelpers.Safe(delegate
        {
            cInfo.SetFoP("LoaderAssemblyName", Svc.PluginInterface.InternalName);
        });
        Svc.Commands.AddHandler(command, cInfo);
    }
}
