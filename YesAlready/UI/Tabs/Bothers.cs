using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using ECommons.SimpleGui;
using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Linq;
using System.Reflection;
using YesAlready.Interface;

namespace YesAlready.UI.Tabs;

public static class Bothers
{
    private static readonly (BotherCategory Category, string Header)[] CategoryHeaders =
    [
        (BotherCategory.Desynthesis, "Desynthesis and Aetherial Reduction"),
        (BotherCategory.Materia, "Materia"),
        (BotherCategory.Retainers, "Retainers and Submersibles"),
        (BotherCategory.Duties, "Duties"),
        (BotherCategory.PvP, "PvP"),
        (BotherCategory.Minigames, "Minigames and Special Events"),
        (BotherCategory.Shops, "Shops"),
        (BotherCategory.Glamour, "Glamour"),
        (BotherCategory.Other, "Other"),
        (BotherCategory.Forays, "Forays"),
    ];

    public static void Draw()
    {
        using var tab = ImRaii.TabItem("Bothers");
        if (!tab) return;
        using var idScope = ImRaii.PushId("BothersOptions");
        using var child = ImRaii.Child("BothersContent", System.Numerics.Vector2.Zero);
        if (!child) return;

        var bothersByCategory = FeatureRegistry.Get().GetBothers().GroupBy(x => x.Bother.Category).ToDictionary(g => g.Key, g => g.ToList());
        foreach (var (category, header) in CategoryHeaders)
        {
            if (!bothersByCategory.TryGetValue(category, out var bothers) || bothers.Count == 0)
                continue;

            if (!ImGui.CollapsingHeader(header))
                continue;

            foreach (var (feature, bother) in bothers)
                DrawBother(feature, bother);
        }
    }

    internal static void DrawBother(AddonFeature feature, BotherAttribute bother)
    {
        if (bother.RequiresEnabledProperty is { } required && !GetConfigBool(required))
            return;

        using var indent = bother.RequiresEnabledProperty is not null ? ImRaii.PushIndent() : null;
        using var id = ImRaii.PushId($"{feature.Key}_{bother.ConfigProperty}");

        var value = GetConfigBool(bother.ConfigProperty);
        if (ImGui.Checkbox(bother.Label is string label ? $"{feature.Key} - {label}" : feature.Key, ref value))
        {
            SetConfigBool(bother.ConfigProperty, value);

            if (value && bother.MutuallyExclusiveWith is { } exclusive)
                SetConfigBool(exclusive, false);

            // ContentsFinderConfirm / OneTimeConfirm coupling
            if (bother.ConfigProperty == nameof(Configuration.ContentsFinderConfirmEnabled) && !value)
                SetConfigBool(nameof(Configuration.ContentsFinderOneTimeConfirmEnabled), false);
            else if (bother.ConfigProperty == nameof(Configuration.ContentsFinderOneTimeConfirmEnabled) && value)
                SetConfigBool(nameof(Configuration.ContentsFinderConfirmEnabled), true);

            C.Save();
        }

        ImGuiX.IndentedTextColored(bother.Description);

        if (bother.ConfigProperty == nameof(Configuration.ItemInspectionResultEnabled) && value)
        {
            using var rateIndent = ImRaii.PushIndent();
            var rateLimit = C.ItemInspectionResultRateLimiter;
            if (ImGui.InputInt("##ItemInspectionRateLimiter", ref rateLimit))
            {
                C.ItemInspectionResultRateLimiter = rateLimit;
                C.Save();
            }
            ImGuiX.IndentedTextColored("Rate limiter (pause after N items, 0 to disable).");
        }

        if (bother.ConfigProperty == nameof(Configuration.TradeMultiple) && value)
            DrawTradeMultipleSettings();

        if (bother.ConfigProperty == nameof(Configuration.HousingGardeningAutoSelect) && value)
            DrawAutoSelectGardeningSettings();
    }

    private static void DrawTradeMultipleSettings()
    {
        using var settingsIndent = ImRaii.PushIndent();

        var mode = (int)C.TransmuteMode;
        var modes = new[] { "All Same", "All Different" };
        if (ImGui.Combo("Mode##TransmuteMode", ref mode, modes, modes.Length))
        {
            C.TransmuteMode = (Configuration.TradeMultipleMode)mode;
            C.Save();
        }
        ImGuiX.IndentedTextColored("Whether to submit all of the same materia at once or try to use all different.");

        if (C.TransmuteMode == Configuration.TradeMultipleMode.AllDifferent)
        {
            var requireUnique = C.TradeMultipleRequireUnique;
            if (ImGui.Checkbox("Require all different", ref requireUnique))
            {
                C.TradeMultipleRequireUnique = requireUnique;
                C.Save();
            }
            ImGuiX.IndentedTextColored("If enabled, stop instead of padding with duplicate types when fewer than five unique materia are available.");
        }

        var count = C.TradeMultipleBlacklistItemIds.Count;
        if (ImGui.Button($"Blacklist materia ({count})##TransmuteBlacklistOpen"))
            EzConfigGui.GetWindow<MateriaBlacklistWindow>()?.Toggle();
    }

    private static void DrawAutoSelectGardeningSettings()
    {
        using var indent = ImRaii.PushIndent();

        var onlyInv = C.HousingGardeningOnlyInventory;
        if (ImGui.Checkbox("Show only inventory items##GardeningOnlyInv", ref onlyInv))
        {
            C.HousingGardeningOnlyInventory = onlyInv;
            C.Save();
        }

        DrawGardeningItemCombo("Soil", filterGroup: 21, selectedId: C.HousingGardeningSoil, id =>
        {
            C.HousingGardeningSoil = id;
            C.Save();
        });
        DrawGardeningItemCombo("Seed", filterGroup: 20, selectedId: C.HousingGardeningSeed, id =>
        {
            C.HousingGardeningSeed = id;
            C.Save();
        });

        var fallback = C.HousingGardeningFallback;
        if (ImGui.Checkbox("Fallback to first available", ref fallback))
        {
            C.HousingGardeningFallback = fallback;
            C.Save();
        }
        ImGuiX.IndentedTextColored("If the preferred soil/seed is missing, use the first matching item in inventory.");

        var autoConfirm = C.HousingGardeningConfirm;
        if (ImGui.Checkbox("Auto confirm##GardeningConfirm", ref autoConfirm))
        {
            C.HousingGardeningConfirm = autoConfirm;
            C.Save();
        }
        ImGuiX.IndentedTextColored("Plant the seeds after selecting them.");

        var includeFert = C.HousingGardeningUseFertilizer;
        if (ImGui.Checkbox("Auto-select fertilizer", ref includeFert))
        {
            C.HousingGardeningUseFertilizer = includeFert;
            C.Save();
        }
        ImGuiX.IndentedTextColored("When the fertilize inventory opens, select the preferred fertilizer.");

        if (C.HousingGardeningUseFertilizer)
        {
            DrawGardeningItemCombo("Fertilizer", filterGroup: 22, selectedId: C.HousingGardeningFertilizer, id =>
            {
                C.HousingGardeningFertilizer = id;
                C.Save();
            });
        }
    }

    private static unsafe void DrawGardeningItemCombo(string label, byte filterGroup, uint selectedId, Action<uint> setSelected)
    {
        var sheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Item>();
        if (sheet == null)
            return;

        var preview = selectedId != 0 && sheet.TryGetRow(selectedId, out var selected) ? selected.Name.ExtractText() : "(none)";
        using var combo = ImRaii.Combo(label, preview);
        if (!combo)
            return;

        if (ImGui.Selectable("(none)", selectedId == 0))
            setSelected(0);

        var im = InventoryManager.Instance();
        foreach (var item in sheet)
        {
            if (item.ItemUICategory.RowId != 82 || item.FilterGroup != filterGroup)
                continue;

            if (C.HousingGardeningOnlyInventory && im->GetInventoryItemCount(item.RowId) == 0)
                continue;

            var name = item.Name.ExtractText();
            if (string.IsNullOrEmpty(name))
                continue;

            if (ImGui.Selectable(name, selectedId == item.RowId))
                setSelected(item.RowId);

            if (selectedId == item.RowId)
                ImGui.SetItemDefaultFocus();
        }
    }

    internal static bool GetConfigBool(string propertyName)
    {
        var prop = typeof(Configuration).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)
            ?? throw new InvalidOperationException($"Configuration has no property '{propertyName}'.");
        return (bool)prop.GetValue(C)!;
    }

    private static void SetConfigBool(string propertyName, bool value)
    {
        var prop = typeof(Configuration).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)
            ?? throw new InvalidOperationException($"Configuration has no property '{propertyName}'.");
        prop.SetValue(C, value);
    }
}
