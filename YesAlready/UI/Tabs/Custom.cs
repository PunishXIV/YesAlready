using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using System.Collections.Generic;
using static YesAlready.Configuration;

namespace YesAlready.UI.Tabs;
public static class Custom
{
    public static void Draw()
    {
        using var tab = ImRaii.TabItem("Custom");
        if (!tab) return;
        using var idScope = ImRaii.PushId($"CustomBothers");

        if (ImGui.Button("add"))
        {
            P.Config.CustomBothers.Add(new CustomBother
            {
                Addon = "AddonName",
                CallbackParams = [-1]
            });
            P.Config.Save();
        }

        foreach (var bother in P.Config.CustomBothers)
        {
            var name = bother.Addon;
            if (ImGui.InputText("Addon Name", ref name, 50))
            {
                bother.Addon = name;
                P.Config.Save();
            }

            var args = string.Empty;
            if (ImGui.InputText("Command", ref args, 150))
            {
                bother.CallbackParams = ParseArgs(args);
                P.Config.Save();
            }

            ImGui.SameLine();
            if (ImGui.Button("remove"))
            {
                P.Config.CustomBothers.Remove(bother);
                P.Config.Save();
            }
        }
    }

    private static object[] ParseArgs(string args)
    {
        var rawValues = args.Split(' ');
        var valueArgs = new List<object>();

        var current = "";
        var inQuotes = false;

        for (var i = 0; i < rawValues.Length; i++)
        {
            if (!inQuotes)
            {
                if (rawValues[i].StartsWith('\"'))
                {
                    inQuotes = true;
                    current = rawValues[i].TrimStart('"');
                }
                else
                {
                    if (int.TryParse(rawValues[i], out var iValue)) valueArgs.Add(iValue);
                    else if (uint.TryParse(rawValues[i].TrimEnd('U', 'u'), out var uValue)) valueArgs.Add(uValue);
                    else if (bool.TryParse(rawValues[i], out var bValue)) valueArgs.Add(bValue);
                    else valueArgs.Add(rawValues[i]);
                }
            }
            else
            {
                if (rawValues[i].EndsWith('\"'))
                {
                    inQuotes = false;
                    current += " " + rawValues[i].TrimEnd('"');
                    valueArgs.Add(current);
                    current = "";
                }
                else
                    current += " " + rawValues[i];
            }
        }
        return [.. valueArgs];
    }
}
