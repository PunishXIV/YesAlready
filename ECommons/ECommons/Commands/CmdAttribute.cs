using ECommons.LanguageHelpers;
using System;

namespace ECommons.Commands;

[AttributeUsage(AttributeTargets.Method)]
public class CmdAttribute : Attribute
{
    public string Command { get; }
    public string HelpMessage { get; }
    public bool ShowInHelp { get; }
    public bool ShowInHelpPanel { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="command"></param>
    /// <param name="helpMessage"></param>
    /// <param name="showInHelp"></param>
    /// <param name="showInHelpPanel">Whether show the major command help on the <seealso cref="ImGuiNET.ImGui"/> window</param>
    public CmdAttribute(string command, string helpMessage = "", bool showInHelp = true, bool showInHelpPanel = true)
    {
        Command = command;
        HelpMessage = helpMessage.Loc();
        ShowInHelp = showInHelp;
        ShowInHelpPanel = showInHelpPanel;
    }
}
