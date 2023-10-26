using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Interface.Style;
using Dalamud.Interface.Utility;
using Dalamud.Logging;
using ECommons.DalamudServices;
using ECommons.Reflection;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

namespace ECommons.ImGuiMethods;

public static unsafe partial class ImGuiEx
{
    public static bool CollapsingHeader(string text, Vector4? col = null)
    {
        if (col != null) ImGui.PushStyleColor(ImGuiCol.Text, col.Value);
        var ret = ImGui.CollapsingHeader(text);
        if (col != null) ImGui.PopStyleColor();
        return ret;
    }


    /// <summary>
    /// Provides a button that can be used to switch <see langword="bool"/>? variables. Left click - to toggle between <see langword="true"/> and <see langword="null"/>, right click - to toggle between <see langword="false"/> and <see langword="null"/>.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="TrueColor">Color when <paramref name="value"/> is true</param>
    /// <param name="FalseColor">Color when <paramref name="value"/> is false</param>
    /// <param name="smallButton">Whether a button should be small</param>
    /// <returns></returns>
    public static bool ButtonCheckbox(string name, ref bool? value, Vector4? TrueColor = null, Vector4? FalseColor = null, bool smallButton = false)
    {
        TrueColor ??= EColor.Green;
        FalseColor ??= EColor.Red;
        var col = value;
        var ret = false;
        if (col == true)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, TrueColor.Value);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, TrueColor.Value);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, TrueColor.Value);
        }
        else if(col == false)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, FalseColor.Value);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, FalseColor.Value);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, FalseColor.Value);
        }
        if (smallButton ? ImGui.SmallButton(name) : ImGui.Button(name))
        {
            if (value == null || value == false)
            {
                value = true;
            }
            else
            {
                value = false;
            }
            ret = true;
        }
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            if (value == null || value == true)
            {
                value = false;
            }
            else
            {
                value = true;
            }
            ret = true;
        }
        if (col != null) ImGui.PopStyleColor(3);
        return ret;
    }
    
    /// <summary>
    /// Converts RGB color to <see cref="Vector4"/> for ImGui
    /// </summary>
    /// <param name="col">Color in format 0xRRGGBB</param>
    /// <param name="alpha">Optional transparency value between 0 and 1</param>
    /// <returns>Color in <see cref="Vector4"/> format ready to be used with <see cref="ImGui"/> functions</returns>
    public static Vector4 Vector4FromRGB(uint col, float alpha = 1.0f)
    {
        byte* bytes = (byte*)&col;
        return new Vector4((float)bytes[2] / 255f, (float)bytes[1] / 255f, (float)bytes[0] / 255f, alpha);
    }


    /// <summary>
    /// Converts RGBA color to <see cref="Vector4"/> for ImGui
    /// </summary>
    /// <param name="col">Color in format 0xRRGGBBAA</param>
    /// <returns>Color in <see cref="Vector4"/> format ready to be used with <see cref="ImGui"/> functions</returns>
    public static Vector4 Vector4FromRGBA(uint col)
    {
        byte* bytes = (byte*)&col;
        return new Vector4((float)bytes[3] / 255f, (float)bytes[2] / 255f, (float)bytes[1] / 255f, (float)bytes[0] / 255f);
    }

    /// <summary>
    /// Draws a button that acts like a checkbox.
    /// </summary>
    /// <param name="name">Button text</param>
    /// <param name="value">Value</param>
    /// <param name="smallButton">Whether button should be small</param>
    /// <returns>true when clicked, otherwise false</returns>
    public static bool ButtonCheckbox(string name, ref bool value, bool smallButton = false) => ButtonCheckbox(name, ref value, EColor.Red, smallButton);

    /// <summary>
    /// Draws a button that acts like a checkbox.
    /// </summary>
    /// <param name="name">Button text</param>
    /// <param name="value">Value</param>
    /// <param name="color">Active button color</param>
    /// <param name="smallButton">Whether button should be small</param>
    /// <returns>true when clicked, otherwise false</returns>
    public static bool ButtonCheckbox(string name, ref bool value, uint color, bool smallButton = false) => ButtonCheckbox(name, ref value, color.ToVector4(), smallButton);

    /// <summary>
    /// Draws a button that acts like a checkbox.
    /// </summary>
    /// <param name="name">Button text</param>
    /// <param name="value">Value</param>
    /// <param name="color">Active button color</param>
    /// <param name="smallButton">Whether button should be small</param>
    /// <returns>true when clicked, otherwise false</returns>
    public static bool ButtonCheckbox(string name, ref bool value, Vector4 color, bool smallButton = false)
    {
        var col = value;
        var ret = false;
        if (col)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, color);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, color);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, color);
        }
        if (smallButton?ImGui.SmallButton(name):ImGui.Button(name))
        {
            value = !value;
            ret = true;
        }
        if (col) ImGui.PopStyleColor(3);
        return ret;
    }

    public static bool CollectionButtonCheckbox<T>(string name, T value, HashSet<T> collection, bool smallButton = false) => CollectionButtonCheckbox(name, value, collection, EColor.Red, smallButton);

    public static bool CollectionButtonCheckbox<T>(string name, T value, HashSet<T> collection, Vector4 color, bool smallButton = false)
    {
        var col = collection.Contains(value);
        var ret = false;
        if (col)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, color);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, color);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, color);
        }
        if (smallButton ? ImGui.SmallButton(name) : ImGui.Button(name))
        {
            if (col)
            {
                collection.Remove(value);
            }
            else
            {
                collection.Add(value);
            }
            ret = true;
        }
        if (col) ImGui.PopStyleColor(3);
        return ret;
    }

    /// <summary>
    /// Draws two radio buttons for a boolean value.
    /// </summary>
    /// <param name="labelTrue">True choice radio button text</param>
    /// <param name="labelFalse">False choice radio button text</param>
    /// <param name="value">Value</param>
    /// <param name="sameLine">Whether to draw radio buttons on the same line</param>
    /// <param name="prefix">Will be invoked before each radio button draw</param>
    /// <param name="suffix">Will be invoked after each radio button draw</param>
    public static void RadioButtonBool(string labelTrue, string labelFalse, ref bool value, bool sameLine = false, Action prefix = null, Action suffix = null)
    {
        prefix?.Invoke();
        if (ImGui.RadioButton(labelTrue, value)) value = true;
        suffix?.Invoke();
        if (sameLine) ImGui.SameLine();
        prefix?.Invoke();
        if (ImGui.RadioButton(labelFalse, !value)) value = false;
        suffix?.Invoke();
    }

    /// <summary>
    /// Draws equally sized columns without ability to resize
    /// </summary>
    /// <param name="id">Unique ImGui ID</param>
    /// <param name="values">List of actions for each column</param>
    public static void EzTableColumns(string id, Action[] values)
    {
        if (values.Length == 1)
        {
            GenericHelpers.Safe(values[0]);
        }
        else
        {
            if (ImGui.BeginTable(id, values.Length, ImGuiTableFlags.SizingStretchSame))
            {
                foreach (Action action in values)
                {
                    ImGui.TableNextColumn();
                    GenericHelpers.Safe(action);
                }
                ImGui.EndTable();
            }
        }
    }

    /// <summary>
    /// Button that is disabled unless CTRL key is held
    /// </summary>
    /// <param name="text">Button ID</param>
    /// <param name="affix">Button affix</param>
    /// <returns></returns>
    public static bool ButtonCtrl(string text, string affix = " (Hold CTRL)")
    {
        var disabled = !ImGui.GetIO().KeyCtrl;
        if (disabled)
        {
            ImGui.BeginDisabled();
        }
        var name = string.Empty;
        if (text.Contains($"###"))
        {
            var p = text.Split($"###");
            name = $"{p[0]}{affix}###{p[1]}";
        }
        else if (text.Contains($"##"))
        {
            var p = text.Split($"##");
            name = $"{p[0]}{affix}##{p[1]}";
        }
        else
        {
            name = $"{text}{affix}";
        }
        var ret = ImGui.Button(name);
        if (disabled)
        {
            ImGui.EndDisabled();
        }
        return ret;
    }

    public static bool BeginPopupNextToElement(string popupId)
    {
        ImGui.SameLine(0, 0);
        var pos = ImGui.GetCursorScreenPos();
        ImGui.Dummy(Vector2.Zero);
        ImGui.SetNextWindowPos(pos, ImGuiCond.Appearing);
        return ImGui.BeginPopup(popupId);
    }

    [Obsolete("Please switch to CollectionCheckbox")]
    public static bool HashSetCheckbox<T>(string label, T value, HashSet<T> collection) => CollectionCheckbox(label, value, collection);

    public static bool CollectionCheckbox<T>(string label, T value, HashSet<T> collection)
    {
        var x = collection.Contains(value);
        if (ImGui.Checkbox(label, ref x))
        {
            if (x)
            {
                collection.Add(value);
            }
            else
            {
                collection.Remove(value);
            }
            return true;
        }
        return false;
    }

    public record HeaderIconOptions
    {
        public Vector2 Offset { get; init; } = Vector2.Zero;
        public ImGuiMouseButton MouseButton { get; init; } = ImGuiMouseButton.Left;
        public string Tooltip { get; init; } = string.Empty;
        public uint Color { get; init; } = 0xFFFFFFFF;
        public bool ToastTooltipOnClick { get; init; } = false;
        public ImGuiMouseButton ToastTooltipOnClickButton { get; init; } = ImGuiMouseButton.Left;
    }

    private static uint headerLastWindowID = 0;
    private static ulong headerLastFrame = 0;
    private static float headerCurrentPos = 0;
    private static float headerImGuiButtonWidth = 0;

    public static bool AddHeaderIcon(string id, FontAwesomeIcon icon, HeaderIconOptions options = null)
    {
        if (ImGui.IsWindowCollapsed()) return false;

        var scale = ImGuiHelpers.GlobalScale;
        var currentID = ImGui.GetID(0);
        if (currentID != headerLastWindowID || headerLastFrame != Svc.PluginInterface.UiBuilder.FrameCount)
        {
            headerLastWindowID = currentID;
            headerLastFrame = Svc.PluginInterface.UiBuilder.FrameCount;
            headerCurrentPos = 0.25f * ImGui.GetStyle().FramePadding.Length();
            if (!GetCurrentWindowFlags().HasFlag(ImGuiWindowFlags.NoTitleBar))
                headerCurrentPos = 1;
            headerImGuiButtonWidth = 0f;
            if (CurrentWindowHasCloseButton())
                headerImGuiButtonWidth += 17 * scale;
            if (!GetCurrentWindowFlags().HasFlag(ImGuiWindowFlags.NoCollapse))
                headerImGuiButtonWidth += 17 * scale;
        }

        options ??= new();
        var prevCursorPos = ImGui.GetCursorPos();
        var buttonSize = new Vector2(20 * scale);
        var buttonPos = new Vector2((ImGui.GetWindowWidth() - buttonSize.X - headerImGuiButtonWidth * scale * headerCurrentPos) - (ImGui.GetStyle().FramePadding.X * scale), ImGui.GetScrollY() + 1);
        ImGui.SetCursorPos(buttonPos);
        var drawList = ImGui.GetWindowDrawList();
        drawList.PushClipRectFullScreen();

        var pressed = false;
        ImGui.InvisibleButton(id, buttonSize);
        var itemMin = ImGui.GetItemRectMin();
        var itemMax = ImGui.GetItemRectMax();
        var halfSize = ImGui.GetItemRectSize() / 2;
        var center = itemMin + halfSize;
        if (ImGui.IsWindowHovered() && ImGui.IsMouseHoveringRect(itemMin, itemMax, false))
        {
            if (!string.IsNullOrEmpty(options.Tooltip))
                ImGui.SetTooltip(options.Tooltip);
            ImGui.GetWindowDrawList().AddCircleFilled(center, halfSize.X, ImGui.GetColorU32(ImGui.IsMouseDown(ImGuiMouseButton.Left) ? ImGuiCol.ButtonActive : ImGuiCol.ButtonHovered));
            if (ImGui.IsMouseReleased(options.MouseButton))
                pressed = true;
            if (options.ToastTooltipOnClick && ImGui.IsMouseReleased(options.ToastTooltipOnClickButton))
                Svc.PluginInterface.UiBuilder.AddNotification(options.Tooltip!, null, NotificationType.Info);
        }

        ImGui.SetCursorPos(buttonPos);
        ImGui.PushFont(UiBuilder.IconFont);
        var iconString = icon.ToIconString();
        drawList.AddText(UiBuilder.IconFont, ImGui.GetFontSize(), itemMin + halfSize - ImGui.CalcTextSize(iconString) / 2 + options.Offset, options.Color, iconString);
        ImGui.PopFont();

        ImGui.PopClipRect();
        ImGui.SetCursorPos(prevCursorPos);

        return pressed;
    }

    public static bool CollectionCheckbox<T>(string label, T value, List<T> collection, bool inverted = false)
    {
        var x = collection.Contains(value);
        if (inverted) x = !x;
        if (ImGui.Checkbox(label, ref x))
        {
            if (inverted) x = !x;
            if (x)
            {
                collection.Add(value);
            }
            else
            {
                collection.RemoveAll(x => x.Equals(value));
            }
            return true;
        }
        return false;
    }

    public static Vector4 MutateColor(ImGuiCol col, byte r, byte g, byte b)
    {
        return ImGui.GetStyle().Colors[(int)col] with { X = (float)r / 255f, Y = (float)g / 255f, Z = (float)b / 255f };
    }

    /// <summary>
    /// Displays ImGui.SliderFloat for internal int value.
    /// </summary>
    /// <param name="id">ImGui ID</param>
    /// <param name="value">Integer value</param>
    /// <param name="min">Minimal value</param>
    /// <param name="max">Maximum value</param>
    /// <param name="divider">Value is divided by divider before being presented to user</param>
    /// <returns></returns>
    public static bool SliderIntAsFloat(string id, ref int value, int min, int max, float divider = 1000)
    {
        var f = (float)value / divider;
        var ret = ImGui.SliderFloat(id, ref f, (float)min / divider, (float)max / divider);
        if (ret)
        {
            value = (int)(f * divider);
        }
        return ret;
    }

    public static bool IsKeyPressed(int key, bool repeat)
    {
        byte repeat2 = (byte)(repeat ? 1 : 0);
        return ImGuiNative.igIsKeyPressed((ImGuiKey)key, repeat2) != 0;
    }

    public static void TextUnderlined(uint color, string text)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, color);
        TextUnderlined(text);
        ImGui.PopStyleColor();
    }

    public static void TextUnderlined(Vector4 color, string text)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, color);
        TextUnderlined(text);
        ImGui.PopStyleColor();
    }

    public static void TextUnderlined(string text)
    {
        var size = ImGui.CalcTextSize(text);
        var cur = ImGui.GetCursorScreenPos();
        cur.Y += size.Y;
        ImGui.GetWindowDrawList().PathLineTo(cur);
        cur.X += size.X;
        ImGui.GetWindowDrawList().PathLineTo(cur);
        ImGui.GetWindowDrawList().PathStroke(ImGuiColors.DalamudWhite.ToUint());
        ImGuiEx.Text(text);
    }

    public static float GetWindowContentRegionWidth()
    {
        return ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X;
    }

    public static void Spacing(float pix = 10f, bool accountForScale = true)
    {
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (accountForScale ? pix : pix * ImGuiHelpers.GlobalScale));
    }

    public static float Scale(this float f)
    {
        return f * ImGuiHelpers.GlobalScale;
    }

    public static void SetTooltip(string text)
    {
        ImGui.BeginTooltip();
        ImGui.TextUnformatted(text);
        ImGui.EndTooltip();
    }

    static readonly Dictionary<string, float> CenteredLineWidths = new();
    public static void ImGuiLineCentered(string id, Action func)
    {
        if (CenteredLineWidths.TryGetValue(id, out var dims))
        {
            ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X / 2 - dims / 2);
        }
        var oldCur = ImGui.GetCursorPosX();
        func();
        ImGui.SameLine(0, 0);
        CenteredLineWidths[id] = ImGui.GetCursorPosX() - oldCur;
        ImGui.Dummy(Vector2.Zero);
    }

    public static void SetNextItemFullWidth(int mod = 0)
    {
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X + mod);
    }

    public static void SetNextItemWidth(float percent, int mod = 0)
    {
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * percent + mod);
    }

    static Dictionary<string, float> InputWithRightButtonsAreaValues = new();
    /// <summary>
    /// Convenient way to display stretched input with button or other elements on it's right side.
    /// </summary>
    /// <param name="id">Unique ID</param>
    /// <param name="inputAction">A single element that accepts transformation by ImGui.SetNextItemWidth method</param>
    /// <param name="rightAction">A line of elements on the right side. Can contain multiple elements but only one line.</param>
    public static void InputWithRightButtonsArea(string id, Action inputAction, Action rightAction)
    {
        if (InputWithRightButtonsAreaValues.ContainsKey(id))
        {
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - InputWithRightButtonsAreaValues[id]);
        }
        inputAction();
        ImGui.SameLine();
        var cur1 = ImGui.GetCursorPosX();
        rightAction();
        ImGui.SameLine(0, 0);
        InputWithRightButtonsAreaValues[id] = ImGui.GetCursorPosX() - cur1 + ImGui.GetStyle().ItemSpacing.X;
        ImGui.Dummy(Vector2.Zero);
    }

    static Dictionary<string, Box<string>> InputListValuesString = new();
    public static void InputListString(string name, List<string> list, Dictionary<string, string> overrideValues = null)
    {
        if (!InputListValuesString.ContainsKey(name)) InputListValuesString[name] = new("");
        InputList(name, list, overrideValues, delegate
        {
            var buttonSize = ImGuiHelpers.GetButtonSize("Add");
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - buttonSize.X - ImGui.GetStyle().ItemSpacing.X);
            ImGui.InputText($"##{name.Replace("#", "_")}", ref InputListValuesString[name].Value, 100);
            ImGui.SameLine();
            if (ImGui.Button("Add"))
            {
                list.Add(InputListValuesString[name].Value);
                InputListValuesString[name].Value = "";
            }
        });
    }

    static Dictionary<string, Box<uint>> InputListValuesUint = new();
    public static void InputListUint(string name, List<uint> list, Dictionary<uint, string> overrideValues = null)
    {
        if (!InputListValuesUint.ContainsKey(name)) InputListValuesUint[name] = new(0);
        InputList(name, list, overrideValues, delegate
        {
            var buttonSize = ImGuiHelpers.GetButtonSize("Add");
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - buttonSize.X - ImGui.GetStyle().ItemSpacing.X);
            ImGuiEx.InputUint($"##{name.Replace("#", "_")}", ref InputListValuesUint[name].Value);
            ImGui.SameLine();
            if (ImGui.Button("Add"))
            {
                list.Add(InputListValuesUint[name].Value);
                InputListValuesUint[name].Value = 0;
            }
        });
    }

    public static void InputList<T>(string name, List<T> list, Dictionary<T, string> overrideValues, Action addFunction)
    {
        var text = list.Count == 0 ? "- No values -" : (list.Count == 1 ? $"{(overrideValues != null && overrideValues.ContainsKey(list[0]) ? overrideValues[list[0]] : list[0])}" : $"- {list.Count} elements -");
        if (ImGui.BeginCombo(name, text))
        {
            addFunction();
            var rem = -1;
            for (var i = 0; i < list.Count; i++)
            {
                var id = $"{name}ECommonsDeleItem{i}";
                var x = list[i];
                ImGui.Selectable($"{(overrideValues != null && overrideValues.ContainsKey(x) ? overrideValues[x] : x)}");
                if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                {
                    ImGui.OpenPopup(id);
                }
                if (ImGui.BeginPopup(id))
                {
                    if (ImGui.Selectable("Delete##ECommonsDeleItem"))
                    {
                        rem = i;
                    }
                    if (ImGui.Selectable("Clear (hold shift+ctrl)##ECommonsDeleItem")
                        && ImGui.GetIO().KeyShift && ImGui.GetIO().KeyCtrl)
                    {
                        rem = -2;
                    }
                    ImGui.EndPopup();
                }
            }
            if (rem > -1)
            {
                list.RemoveAt(rem);
            }
            if (rem == -2)
            {
                list.Clear();
            }
            ImGui.EndCombo();
        }
    }

    public static void WithTextColor(Vector4 col, Action func)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, col);
        GenericHelpers.Safe(func);
        ImGui.PopStyleColor();
    }

    public static void Tooltip(string s)
    {
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(s);
        }
    }

    /// <summary>
    /// Aligns text vertically to a standard size button.
    /// </summary>
    /// <param name="col">Color</param>
    /// <param name="s">Text</param>
    public static void TextV(Vector4? col, string s)
    {
        if(col != null) ImGui.PushStyleColor(ImGuiCol.Text, col.Value);
        ImGuiEx.TextV(s);
        if(col != null) ImGui.PopStyleColor();
    }

    /// <summary>
    /// Aligns text vertically to a standard size button.
    /// </summary>
    /// <param name="s">Text</param>
    public static void TextV(string s)
    {
        var cur = ImGui.GetCursorPos();
        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0);
        ImGui.Button("");
        ImGui.PopStyleVar();
        ImGui.SameLine();
        ImGui.SetCursorPos(cur);
        ImGui.TextUnformatted(s);
    }

    public static void Text(string s)
    {
        ImGui.TextUnformatted(s);
    }

    public static void Text(Vector4 col, string s)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, col);
        ImGui.TextUnformatted(s);
        ImGui.PopStyleColor();
    }

    public static void Text(uint col, string s)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, col);
        ImGui.TextUnformatted(s);
        ImGui.PopStyleColor();
    }

    public static void TextWrapped(string s)
    {
        ImGui.PushTextWrapPos();
        ImGui.TextUnformatted(s);
        ImGui.PopTextWrapPos();
    }

    public static void TextWrapped(Vector4 col, string s)
    {
        ImGui.PushTextWrapPos(0);
        ImGuiEx.Text(col, s);
        ImGui.PopTextWrapPos();
    }

    public static void TextWrapped(uint col, string s)
    {
        ImGui.PushTextWrapPos();
        ImGuiEx.Text(col, s);
        ImGui.PopTextWrapPos();
    }

    public static Vector4 GetParsedColor(int percent)
    {
        if (percent < 25)
        {
            return ImGuiColors.ParsedGrey;
        }
        else if (percent < 50)
        {
            return ImGuiColors.ParsedGreen;
        }
        else if (percent < 75)
        {
            return ImGuiColors.ParsedBlue;
        }
        else if (percent < 95)
        {
            return ImGuiColors.ParsedPurple;
        }
        else if (percent < 99)
        {
            return ImGuiColors.ParsedOrange;
        }
        else if (percent == 99)
        {
            return ImGuiColors.ParsedPink;
        }
        else if (percent == 100)
        {
            return ImGuiColors.ParsedGold;
        }
        else
        {
            return ImGuiColors.DalamudRed;
        }
    }

    public static void EzTabBar(string id, params (string name, Action function, Vector4? color, bool child)[] tabs) => EzTabBar(id, false, tabs);

    public static void EzTabBar(string id, bool KoFiTransparent, params (string name, Action function, Vector4? color, bool child)[] tabs)
    {
        ImGui.BeginTabBar(id);
        foreach (var x in tabs)
        {
            if (x.name == null) continue;
            if (x.color != null)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, x.color.Value);
            }
            if (ImGui.BeginTabItem(x.name))
            {
                if (x.color != null)
                {
                    ImGui.PopStyleColor();
                }
                if (x.child) ImGui.BeginChild(x.name + "child");
                x.function();
                if (x.child) ImGui.EndChild();
                ImGui.EndTabItem();
            }
            else
            {
                if (x.color != null)
                {
                    ImGui.PopStyleColor();
                }
            }
        }
        if (KoFiTransparent) KoFiButton.RightTransparentTab();
        ImGui.EndTabBar();
    }

    public static void InvisibleButton(int width = 0)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0);
        ImGui.Button(" ");
        ImGui.PopStyleVar();
    }

    public static Dictionary<string, Box<string>> EnumComboSearch = new();
    /// <summary>
    /// Draws an easy combo selector for an enum with a search field for long lists.
    /// </summary>
    /// <typeparam name="T">Enum</typeparam>
    /// <param name="name">ImGui ID</param>
    /// <param name="refConfigField">Value</param>
    /// <param name="names">Optional Name overrides</param>
    public static bool EnumCombo<T>(string name, ref T refConfigField, IDictionary<T, string> names) where T : IConvertible
    {
        return EnumCombo(name, ref refConfigField, null, names);
    }

    /// <summary>
    /// Draws an easy combo selector for an enum with a search field for long lists.
    /// </summary>
    /// <typeparam name="T">Enum</typeparam>
    /// <param name="name">ImGui ID</param>
    /// <param name="refConfigField">Value</param>
    /// <param name="filter">Optional filter</param>
    /// <param name="names">Optional Name overrides</param>
    /// <returns></returns>
    public static bool EnumCombo<T>(string name, ref T refConfigField, Func<T, bool> filter = null, IDictionary<T, string> names = null) where T : IConvertible
    {
        var ret = false;
        if (ImGui.BeginCombo(name, (names != null && names.TryGetValue(refConfigField, out var n)) ? n : refConfigField.ToString().Replace("_", " ")))
        {
            var values = Enum.GetValues(typeof(T));
            Box<string> fltr = null;
            if (values.Length > 10)
            {
                if (!EnumComboSearch.ContainsKey(name)) EnumComboSearch.Add(name, new(""));
                fltr = EnumComboSearch[name];
                ImGuiEx.SetNextItemFullWidth();
                ImGui.InputTextWithHint($"##{name.Replace("#", "_")}", "Filter...", ref fltr.Value, 50);
            }
            foreach (var x in values)
            {
                var equals = EqualityComparer<T>.Default.Equals((T)x, refConfigField);
                var element = (names != null && names.TryGetValue((T)x, out n)) ? n : x.ToString().Replace("_", " ");
                if ((filter == null || filter((T)x))
                    && (fltr == null || element.Contains(fltr.Value, StringComparison.OrdinalIgnoreCase))
                    && ImGui.Selectable(element, equals)
                    )
                {
                    ret = true;
                    refConfigField = (T)x;
                }
                if (ImGui.IsWindowAppearing() && equals) ImGui.SetScrollHereY();
            }
            ImGui.EndCombo();
        }
        return ret;
    }

    public static Dictionary<string, Box<string>> ComboSearch = new();
    public static bool Combo<T>(string name, ref T refConfigField, IEnumerable<T> values, Func<T, bool> filter = null, Dictionary<T, string> names = null)
    {
        var ret = false;
        if (ImGui.BeginCombo(name, (names != null && names.TryGetValue(refConfigField, out var n)) ? n : refConfigField.ToString()))
        {
            Box<string> fltr = null;
            if (values.Count() > 10)
            {
                if (!ComboSearch.ContainsKey(name)) ComboSearch.Add(name, new(""));
                fltr = ComboSearch[name];
                ImGuiEx.SetNextItemFullWidth();
                ImGui.InputTextWithHint($"##{name}fltr", "Filter...", ref fltr.Value, 50);
            }
            foreach (var x in values)
            {
                var equals = EqualityComparer<T>.Default.Equals(x, refConfigField);
                var element = (names != null && names.TryGetValue(x, out n)) ? n : x.ToString();
                if ((filter == null || filter(x))
                    && (fltr == null || element.Contains(fltr.Value, StringComparison.OrdinalIgnoreCase))
                    && ImGui.Selectable(element, equals)
                    )
                {
                    ret = true;
                    refConfigField = x;
                }
                if (ImGui.IsWindowAppearing() && equals) ImGui.SetScrollHereY();
            }
            ImGui.EndCombo();
        }
        return ret;
    }

    public static bool IconButton(FontAwesomeIcon icon, string id = "ECommonsButton", Vector2 size = default)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        var result = ImGui.Button($"{icon.ToIconString()}##{icon.ToIconString()}-{id}", size);
        ImGui.PopFont();
        return result;
    }

    public static bool IconButton(string icon, string id = "ECommonsButton")
    {
        ImGui.PushFont(UiBuilder.IconFont);
        var result = ImGui.Button($"{icon}##{icon}-{id}");
        ImGui.PopFont();
        return result;
    }

    public static Vector2 CalcIconSize(FontAwesomeIcon icon)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        var result = ImGui.CalcTextSize($"{icon.ToIconString()}");
        ImGui.PopFont();
        return result;
    }

    public static float Measure(Action func, bool includeSpacing = true)
    {
        var pos = ImGui.GetCursorPosX();
        func();
        ImGui.SameLine(0, 0);
        var diff = ImGui.GetCursorPosX() - pos;
        ImGui.Dummy(Vector2.Zero);
        return diff + (includeSpacing ? ImGui.GetStyle().ItemSpacing.X : 0);
    }

    public static void InputHex(string name, ref uint hexInt)
    {
        var text = $"{hexInt:X}";
        if (ImGui.InputText(name, ref text, 8))
        {
            if (uint.TryParse(text.Replace("0x", ""), NumberStyles.HexNumber, null, out var num))
            {
                hexInt = num;
            }
        }
    }

    public static void InputHex(string name, ref byte hexByte)
    {
        var text = $"{hexByte:X}";
        if (ImGui.InputText(name, ref text, 2))
        {
            if (byte.TryParse(text, NumberStyles.HexNumber, null, out var num))
            {
                hexByte = num;
            }
        }
    }

    public static void InputUint(string name, ref uint uInt)
    {
        var text = $"{uInt}";
        if (ImGui.InputText(name, ref text, 16))
        {
            if (uint.TryParse(text, out var num))
            {
                uInt = num;
            }
        }
    }

    public static void TextCopy(Vector4 col, string text)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, col);
        TextCopy(text);
        ImGui.PopStyleColor();
    }

    public static void TextCopy(string text)
    {
        ImGui.TextUnformatted(text);
        if (ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            ImGui.SetClipboardText(text);
            Svc.PluginInterface.UiBuilder.AddNotification("Text copied to clipboard", null, NotificationType.Success);
        }
    }

    public static void TextWrappedCopy(string text)
    {
        ImGuiEx.TextWrapped(text);
        if (ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            ImGui.SetClipboardText(text);
            Svc.PluginInterface.UiBuilder.AddNotification("Text copied to clipboard", DalamudReflector.GetPluginName(), NotificationType.Success);
        }
    }

    public static void TextWrappedCopy(Vector4 col, string text)
    {
        ImGuiEx.TextWrapped(col, text);
        if (ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            ImGui.SetClipboardText(text);
            Svc.PluginInterface.UiBuilder.AddNotification("Text copied to clipboard", DalamudReflector.GetPluginName(), NotificationType.Success);
        }
    }

    public static void TextCentered(string text)
    {
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X / 2 - ImGui.CalcTextSize(text).X / 2);
        Text(text);
    }

    public static void TextCentered(Vector4 col, string text)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, col);
        TextCentered(text);
        ImGui.PopStyleColor();
    }

    public static void Text(Vector4? col, string text)
    {
        if(col == null)
        {
            Text(text);
        }
        else
        {
            Text(col.Value, text);
        }
    }

    public static void TextCentered(Vector4? col, string text)
    {
        if (col == null) 
        {
            TextCentered(text);
        }
        else 
        {
            TextCentered(col.Value, text);
        }
    }

    public static void ButtonCopy(string buttonText, string copy)
    {
        if (ImGui.Button(buttonText.Replace("$COPY", copy)))
        {
            ImGui.SetClipboardText(copy);
            Svc.PluginInterface.UiBuilder.AddNotification("Text copied to clipboard", null, NotificationType.Success);
        }
    }

    public static void CenterColumnText(string text, bool underlined = false)
    {
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (ImGui.GetColumnWidth() * 0.5f) - (ImGui.CalcTextSize(text).X * 0.5f));
        if (underlined)
            TextUnderlined(text);
        else
            Text(text);
    }

    public static void CenterColumnText(Vector4 colour, string text, bool underlined = false)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, colour);
        CenterColumnText(text, underlined);
        ImGui.PopStyleColor();
    }

    public unsafe static bool BeginTabItem(string label, ImGuiTabItemFlags flags)
    {
        int num = 0;
        byte* ptr;
        if (label != null)
        {
            num = Encoding.UTF8.GetByteCount(label);
            ptr = Allocate(num + 1);
            int utf = GetUtf8(label, ptr, num);
            ptr[utf] = 0;
        }
        else
        {
            ptr = null;
        }

        byte* p_open2 = null;
        byte num2 = ImGuiNative.igBeginTabItem(ptr, p_open2, flags);
        if (num > 2048)
        {
            Free(ptr);
        }
        return num2 != 0;
    }

    internal unsafe static byte* Allocate(int byteCount)
    {
        return (byte*)(void*)Marshal.AllocHGlobal(byteCount);
    }

    internal unsafe static void Free(byte* ptr)
    {
        Marshal.FreeHGlobal((IntPtr)ptr);
    }

    internal unsafe static int GetUtf8(string s, byte* utf8Bytes, int utf8ByteCount)
    {
        fixed (char* chars = s)
        {
            return Encoding.UTF8.GetBytes(chars, s.Length, utf8Bytes, utf8ByteCount);
        }
    }
}

[StructLayout(LayoutKind.Explicit)]
public struct ImGuiWindow
{
    [FieldOffset(0xC)] public ImGuiWindowFlags Flags;

    [FieldOffset(0xD5)] public byte HasCloseButton;

    // 0x118 is the start of ImGuiWindowTempData
    [FieldOffset(0x130)] public Vector2 CursorMaxPos;
}

public static partial class ImGuiEx
{
    [LibraryImport("cimgui")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static partial nint igGetCurrentWindow();
    public static unsafe ImGuiWindow* GetCurrentWindow() => (ImGuiWindow*)igGetCurrentWindow();
    public static unsafe ImGuiWindowFlags GetCurrentWindowFlags() => GetCurrentWindow()->Flags;
    public static unsafe bool CurrentWindowHasCloseButton() => GetCurrentWindow()->HasCloseButton != 0;

}

