using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using YesAlready.BaseFeatures;
using YesAlready.Features;
using YesAlready.Utils;
using static YesAlready.Configuration;

namespace YesAlready.UI.Tabs;
public static class Custom
{
    public static void Draw()
    {
        using var tab = ImRaii.TabItem("Custom");
        if (!tab) return;
        using var idScope = ImRaii.PushId($"CustomBothers");

        DrawButtons();

        foreach (var bother in P.Config.CustomBothers.ToList())
        {
            using var id = ImRaii.PushId(P.Config.CustomBothers.IndexOf(bother));
            var name = bother.Addon;
            if (ImGui.InputText("Addon Name", ref name, 50, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                bother.Addon = name;
                P.Config.Save();
                ToggleCustomBothers();
            }

            var args = string.Join(" ", bother.CallbackParams);
            if (ImGui.InputText("Parameters", ref args, 150, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                bother.CallbackParams = ParseArgs(args);
                P.Config.Save();
                ToggleCustomBothers();
            }

            ImGui.SameLine();
            if (ImGuiEx.IconButton(FontAwesomeIcon.Trash, "Remove Entry", id: $"Delete##{P.Config.CustomBothers.IndexOf(bother)}"))
            {
                P.Config.CustomBothers.Remove(bother);
                P.Config.Save();
                ToggleCustomBothers();
            }
        }
    }

    public static void DrawButtons()
    {
        var style = ImGui.GetStyle();
        var newStyle = new Vector2(style.ItemSpacing.X / 2, style.ItemSpacing.Y);
        using var _ = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, newStyle);

        if (ImGuiEx.IconButton(FontAwesomeIcon.Plus, "Add new entry"))
        {
            P.Config.CustomBothers.Add(new CustomBother
            {
                Addon = "AddonName",
                CallbackParams = [-1]
            });
            P.Config.Save();
        }

        var sb = new StringBuilder();
        sb.AppendLine("This section allows you to build custom \"Bothers\".");
        sb.AppendLine("Many bothers are very simple, consisting of a single callback parameter to a given addon when it appears. This is for those types of bothers.");
        sb.AppendLine();
        sb.AppendLine("Callback parameter parsing works the same as in Something Need Doing.");
        sb.AppendLine("Custom bothers are registered via AddonLifeCycle on the PostSetup event.");
        sb.AppendLine();
        sb.AppendLine("Some bothers may require infeasible parameters, waits, or different AddonEvents. Those can still be requested for the normal bother system.");
        sb.AppendLine();
        sb.AppendLine("Example:");
        sb.AppendLine("   AddonName: Character");
        sb.AppendLine("   Parameters: -1");
        sb.AppendLine("   Effect: When opening the Character addon, it will instantly be closed. Probably not useful.");

        ImGui.SameLine();
        ImGuiEx.IconButton(FontAwesomeIcon.QuestionCircle, sb.ToString());
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(sb.ToString());
    }

    private static void ToggleCustomBothers()
    {
        var featureAssembly = Assembly.GetExecutingAssembly();

        foreach (var type in featureAssembly.GetTypes())
        {
            if (typeof(BaseFeature).IsAssignableFrom(type) && !type.IsAbstract && type.Name == nameof(CustomAddonCallbacks))
            {
                var feature = (BaseFeature)Activator.CreateInstance(type);
                feature.Disable();
                feature.Enable();
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
