using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using System;
using System.Linq;
using System.Numerics;
using System.Reflection;
using YesAlready.UI.Tabs;
using BotherEntry = (YesAlready.BaseFeatures.AddonFeature Feature, YesAlready.BaseFeatures.BotherAttribute Bother);

namespace YesAlready.UI;

public sealed class BotherBadges : IDisposable
{
    private const string IconResource = "YesAlready.Assets.yesalready_badge.png";
    private const string PopupId = "BotherBadgePopup";

    private const float BadgeSize = 28f;
    private const float TitleGap = 6f;     // between the end of the title text and the badge
    private const float CornerInset = 8f;  // from the top-left corner, for windows without a title
    private const float OutlineWidth = 1.5f;

    private static readonly Vector4 DimmedTint = new(0.85f, 0.85f, 0.85f, 1f);
    private static readonly Vector4 OutlineColour = new(0f, 0f, 0f, 0.85f);
    private static readonly Vector4 HoveredOutlineColour = new(0f, 0f, 0f, 1f);

    private static readonly Vector2[] OutlineDirections = [new(-1, 0), new(1, 0), new(0, -1), new(0, 1), new(-1, -1), new(1, -1), new(-1, 1), new(1, 1)];

    private sealed record BadgedWindow(string AddonName, BotherEntry[] Bothers)
    {
        public bool AnyBotherEnabled => Bothers.Any(b => b.Bother.ContributesToEnable && Tabs.Bothers.GetConfigBool(b.Bother.ConfigProperty));
    }

    private BadgedWindow[]? _windows;

    private static string? _addonWithOpenPopup;

    public BotherBadges() => Svc.PluginInterface.UiBuilder.Draw += Draw;

    public void Dispose() => Svc.PluginInterface.UiBuilder.Draw -= Draw;

    private BadgedWindow[] Windows => _windows ??= [.. FeatureRegistry.Get().GetBothers()
        .SelectMany(x => x.Feature.AddonNames.Select(addon => (Addon: addon, Entry: (x.Feature, x.Bother))))
        .GroupBy(x => x.Addon)
        .Select(g => new BadgedWindow(g.Key, [.. g.Select(x => x.Entry)]))];

    private unsafe void Draw()
    {
        if (!C.ShowBotherBadges) return;

        foreach (var window in Windows)
        {
            if (!GenericHelpers.TryGetAddonByName<AtkUnitBase>(window.AddonName, out var addon) || !addon->IsVisible || addon->RootNode == null)
                continue;

            if (C.HideBadgeWhenBotherEnabled && window.AnyBotherEnabled && window.AddonName != _addonWithOpenPopup)
                continue;

            var scale = addon->Scale;
            var size = new Vector2(BadgeSize * scale);
            DrawBadge(window, GetBadgePosition(addon, size), size, scale);
        }
    }


    private static void DrawBadge(BadgedWindow window, Vector2 pos, Vector2 size, float scale)
    {
        var open = BeginInvisibleWindow($"###YesAlreadyBadge_{window.AddonName}", pos);
        try
        {
            if (!open) return;

            var clicked = ImGui.InvisibleButton("##badge", size);
            var hovered = ImGui.IsItemHovered();
            var active = P.Active && window.AnyBotherEnabled;
            DrawIcon(pos, size, scale, dimmed: !hovered && !active, hovered);

            if (hovered && !ImGui.IsPopupOpen(PopupId))
                DrawTooltip(window);

            if (clicked)
                ImGui.OpenPopup(PopupId);

            DrawPopup(window);
        }
        finally
        {
            ImGui.End();
        }
    }

    private static bool BeginInvisibleWindow(string id, Vector2 pos)
    {
        ImGuiHelpers.ForceNextWindowMainViewport();
        ImGui.SetNextWindowPos(pos);

        using var style = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, Vector2.Zero)
            .Push(ImGuiStyleVar.WindowMinSize, Vector2.One)
            .Push(ImGuiStyleVar.WindowBorderSize, 0f);

        return ImGui.Begin(id,
            ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoSavedSettings
            | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav
            | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoDocking);
    }

    private static void DrawIcon(Vector2 pos, Vector2 size, float scale, bool dimmed, bool strongOutline)
    {
        if (!Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), IconResource).TryGetWrap(out var icon, out _))
            return;

        var drawList = ImGui.GetWindowDrawList();
        var outlineWidth = MathF.Max(1f, OutlineWidth * scale);
        var outlineColour = ImGui.GetColorU32(strongOutline ? HoveredOutlineColour : OutlineColour);

        foreach (var direction in OutlineDirections)
        {
            var nudge = direction * outlineWidth;
            drawList.AddImage(icon.Handle, pos + nudge, pos + size + nudge, Vector2.Zero, Vector2.One, outlineColour);
        }

        drawList.AddImage(icon.Handle, pos, pos + size, Vector2.Zero, Vector2.One, ImGui.GetColorU32(dimmed ? DimmedTint : Vector4.One));
    }

    private static void DrawTooltip(BadgedWindow window)
    {
        using var tooltip = ImRaii.Tooltip();
        using var wrap = ImRaii.TextWrapPos(ImGui.GetFontSize() * 22);

        ImGui.TextUnformatted($"{Name} can automate this window");
        if (!P.Active)
            ImGui.TextColored(ImGuiColors.DalamudRed, "Plugin is currently off or paused");

        foreach (var (_, bother) in window.Bothers.Where(b => b.Bother.ContributesToEnable))
        {
            var on = Bothers.GetConfigBool(bother.ConfigProperty);
            ImGui.TextColored(on ? ImGuiColors.HealerGreen : ImGuiColors.DalamudGrey, $"[{(on ? "On" : "Off")}] {bother.Description}");
        }

        ImGui.TextDisabled("Click to configure");
    }

    private static void DrawPopup(BadgedWindow window)
    {
        using var popup = ImRaii.Popup(PopupId);
        if (!popup)
        {
            if (_addonWithOpenPopup == window.AddonName)
                _addonWithOpenPopup = null;
            return;
        }

        _addonWithOpenPopup = window.AddonName;
        using var wrap = ImRaii.TextWrapPos(ImGui.GetFontSize() * 22);

        ImGui.TextUnformatted(Name);
        ImGui.Separator();
        foreach (var (feature, bother) in window.Bothers)
            Bothers.DrawBother(feature, bother);
    }


    // top left
    private static unsafe Vector2 GetBadgePosition(AtkUnitBase* addon, Vector2 size)
    {
        var scale = addon->Scale;
        var gamePos = TryGetTitleEnd(addon, out var titleEnd)
            ? new Vector2(titleEnd.X + TitleGap * scale, titleEnd.Y - size.Y / 2f) // centred vertically on the title
            : new Vector2(addon->X, addon->Y) + new Vector2(CornerInset * scale);

        return ImGuiHelpers.MainViewport.Pos + gamePos;
    }

    private static unsafe bool TryGetTitleEnd(AtkUnitBase* addon, out Vector2 titleEnd)
    {
        titleEnd = default;
        if (addon->WindowNode == null || addon->WindowNode->Component == null)
            return false;

        var window = (AtkComponentWindow*)addon->WindowNode->Component;
        var found = false;

        for (var i = 0; i < 2; i++) // NodeIds[0] = title, NodeIds[1] = subtitle
        {
            var node = window->UldManager.SearchNodeById((uint)window->NodeIds[i]);
            if (node == null || node->Type != NodeType.Text || !node->IsActuallyVisible)
                continue;

            ushort width, height;
            ((AtkTextNode*)node)->GetTextDrawSize(&width, &height);
            if (width == 0)
                continue;

            var end = new Vector2(node->ScreenX + width * addon->Scale, node->ScreenY + height * addon->Scale / 2f);
            if (!found || end.X > titleEnd.X)
            {
                titleEnd = end;
                found = true;
            }
        }

        return found;
    }

    public static void DrawSettings()
    {
        var show = C.ShowBotherBadges;
        if (ImGui.Checkbox("Window badges", ref show))
        {
            C.ShowBotherBadges = show;
            C.Save();
        }

        ImGui.SameLine();
        ImGui.Dummy(new Vector2(ImGui.GetFrameHeight()));
        DrawIcon(ImGui.GetItemRectMin(), ImGui.GetItemRectSize(), ImGuiHelpers.GlobalScale, dimmed: !C.ShowBotherBadges, strongOutline: false);

        ImGuiX.IndentedTextColored($"Show a {Name} badge on game windows that have a bother. Hover for details, click to configure.");

        if (!C.ShowBotherBadges)
            return;

        using var indent = ImRaii.PushIndent();
        var hideWhenEnabled = C.HideBadgeWhenBotherEnabled;
        if (ImGui.Checkbox("Hide when already enabled", ref hideWhenEnabled))
        {
            C.HideBadgeWhenBotherEnabled = hideWhenEnabled;
            C.Save();
        }
        ImGuiX.IndentedTextColored("Only show the badge on windows where none of its bothers are turned on.");
    }
}
