using ECommons.DalamudServices;
using ImGuiNET;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ECommons.Commands;

public static class CmdManager
{
    static Dictionary<CmdAttribute, SubCmdAttribute[]> _commandsAttribute = new Dictionary<CmdAttribute, SubCmdAttribute[]>();

    internal static void Init()
    {
        var plugin = ECommonsMain.Instance;
        if (plugin == null) return;

        foreach (var m in plugin.GetType().GetRuntimeMethods())
        {
            var types = m.GetParameters();
            if (types.Length != 2
                || types[0].ParameterType != typeof(string)
                || types[1].ParameterType != typeof(string)) continue;

            var cmdAttr = m.GetCustomAttribute<CmdAttribute>();
            if (cmdAttr == null) continue;

            Svc.Commands.AddHandler(cmdAttr.Command, new Dalamud.Game.Command.CommandInfo((string command, string arguments) =>
            {
                m.Invoke(plugin, new object[] { command, arguments });
            })
            {
                HelpMessage = cmdAttr.HelpMessage,
                ShowInHelp = cmdAttr.ShowInHelp,
            });

            _commandsAttribute[cmdAttr] = m.GetCustomAttributes<SubCmdAttribute>().ToArray();
        }
    }

    /// <summary>
    /// Draw the help panel into the <seealso cref="ImGui"/> window.
    /// </summary>
    /// <param name="indent">The indent of value. 0 means no index, -1 means next line.</param>
    public static void DrawHelp(float indent = 0)
    {
        bool isFirst = true;
        foreach (var pair in _commandsAttribute)
        {
            if (isFirst) isFirst = false;
            else ImGui.Spacing();

            if (pair.Key.ShowInHelpPanel)
            {
                DisplayCommandHelp(pair.Key.Command, "", pair.Key.HelpMessage, indent);
            }
            foreach (var sub in pair.Value)
            {
                if (sub == null) continue;
                DisplayCommandHelp(pair.Key.Command, sub.SubCommand, sub.HelpMessage, indent);
            }
        }
    }

    public static void DisplayCommandHelp(string command, string extraCommand = "", string helpMessage = "", float indent = 0)
    {
        if (string.IsNullOrEmpty(command)) return;
        if (!string.IsNullOrEmpty(extraCommand))
        {
            command += " " + extraCommand;
        }

        if (ImGui.Button(command))
        {
            Svc.Commands.ProcessCommand(command);
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip($"Click to execute the command: {command}\nRight-click to copy the command: {command}");

            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
            {
                ImGui.SetClipboardText(command);
            }
        }

        if (!string.IsNullOrEmpty(helpMessage))
        {
            if (indent > 0)
            {
                ImGui.SameLine();
                ImGui.Indent(indent);
            }
            else
            {
                if (indent < 0)
                {
                    ImGui.Text("    ");
                }
                ImGui.SameLine();
            }

            ImGui.Text(" → ");
            ImGui.SameLine();
            ImGui.TextWrapped(helpMessage);

            if (indent > 0)
            {
                ImGui.Unindent(indent);
            }
        }
    }

    internal static void Dispose()
    {
        foreach (var cmd in _commandsAttribute.Keys)
        {
            Svc.Commands.RemoveHandler(cmd.Command);
        }
    }
}
