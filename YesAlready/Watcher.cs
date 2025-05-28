using Dalamud.Game.ClientState.Keys;
using Dalamud.Plugin.Services;
using ECommons.EzHookManager;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace YesAlready;

public class Watcher : IDisposable
{
    private unsafe delegate void* FireCallbackDelegate(AtkUnitBase* atkUnitBase, int valueCount, AtkValue* atkValues, byte updateVisibility);
    [EzHook("E8 ?? ?? ?? ?? 0F B6 E8 8B 44 24 20", detourName: nameof(FireCallbackDetour), true)]
    private readonly EzHook<FireCallbackDelegate> FireCallbackHook = null!;

    private bool _wasDisableKeyPressed;
    private uint _lastTargetId;

    public string LastSeenDialogText { get; set; } = string.Empty;
    public string LastSeenOkText { get; set; } = string.Empty;
    public string LastSeenListSelection { get; set; } = string.Empty;
    public int LastSeenListIndex { get; set; }
    public string LastSeenListTarget { get; set; } = string.Empty;
    public (int Index, string Text)[] LastSeenListEntries { get; set; } = [];
    public string LastSeenTalkTarget { get; set; } = string.Empty;
    public string LastSeenNumericsText { get; set; } = string.Empty;
    public DateTime EscapeLastPressed { get; set; } = DateTime.MinValue;
    public string EscapeTargetName { get; set; } = string.Empty;
    public bool ForcedYesKeyPressed { get; set; }
    public bool ForcedTalkKeyPressed { get; set; }
    public bool DisableKeyPressed { get; set; }
    public LastListEntry? LastSelectedListEntry { get; set; } = new();

    public Watcher()
    {
        EzSignatureHelper.Initialize(this);
        Svc.Framework.Update += FrameworkUpdate;
    }

    public void Dispose() => Svc.Framework.Update -= FrameworkUpdate;

    public class LastListEntry
    {
        public uint TargetDataId { get; set; }
        public ListEntryNode? Node { get; set; }
    }

    private void FrameworkUpdate(IFramework framework)
    {
        if (!P.Active && !_wasDisableKeyPressed) return;
        DisableKeyPressed = C.DisableKey != VirtualKey.NO_KEY && Svc.KeyState[C.DisableKey];

        if (P.Active && DisableKeyPressed && !_wasDisableKeyPressed)
            C.Enabled = false;
        else if (!P.Active && !DisableKeyPressed && _wasDisableKeyPressed)
            C.Enabled = true;

        _wasDisableKeyPressed = DisableKeyPressed;

        ForcedYesKeyPressed = C.ForcedYesKey != VirtualKey.NO_KEY && Svc.KeyState[C.ForcedYesKey];

        ForcedTalkKeyPressed = C.ForcedTalkKey != VirtualKey.NO_KEY && C.SeparateForcedKeys && Svc.KeyState[C.ForcedTalkKey];

        if (Svc.KeyState[VirtualKey.ESCAPE])
        {
            EscapeLastPressed = DateTime.Now;

            var target = Svc.Targets.Target;
            EscapeTargetName = target != null ? target.Name.GetText() : string.Empty;
        }

        if (Svc.Targets.Target is { DataId: var id })
        {
            if (id != _lastTargetId)
                Service.Watcher.LastSelectedListEntry = null;
            _lastTargetId = id;
        }
        else
            Service.Watcher.LastSelectedListEntry = null;
    }

    private unsafe void* FireCallbackDetour(AtkUnitBase* atkUnitBase, int valueCount, AtkValue* atkValues, byte updateVisibility)
    {
        if (atkUnitBase->NameString is not ("SelectString" or "SelectIconString"))
            return FireCallbackHook.Original(atkUnitBase, valueCount, atkValues, updateVisibility);

        try
        {
            var atkValueList = Enumerable.Range(0, valueCount)
                .Select<int, object>(i => atkValues[i].Type switch
                {
                    ValueType.Int => atkValues[i].Int,
                    ValueType.String => Marshal.PtrToStringUTF8(new IntPtr(atkValues[i].String)) ?? string.Empty,
                    ValueType.UInt => atkValues[i].UInt,
                    ValueType.Bool => atkValues[i].Byte != 0,
                    _ => $"Unknown Type: {atkValues[i].Type}"
                })
                .ToList();
            PluginLog.Debug($"[{nameof(Watcher)}] Callback triggered on {atkUnitBase->NameString} with values: {string.Join(", ", atkValueList.Select(value => value.ToString()))}");
            LastSeenListIndex = atkValues[0].Int;
        }
        catch (Exception ex)
        {
            PluginLog.Error($"Exception in {nameof(FireCallbackDetour)}: {ex.Message}");
            return FireCallbackHook.Original(atkUnitBase, valueCount, atkValues, updateVisibility);
        }
        return FireCallbackHook.Original(atkUnitBase, valueCount, atkValues, updateVisibility);
    }
}
