using Dalamud.Hooking;
using ECommons;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Linq;

namespace YesAlready.BaseFeatures;

internal abstract class OnSetupSelectListFeature : BaseFeature, IDisposable
{
    protected OnSetupSelectListFeature() { }

    public Hook<AddonReceiveEventDelegate>? onItemSelectedHook = null;
    public unsafe delegate nint AddonReceiveEventDelegate(AtkEventListener* self, AtkEventType eventType, uint eventParam, AtkEvent* eventData, ulong* inputData);

    public void Dispose()
    {
        onItemSelectedHook?.Disable();
        onItemSelectedHook?.Dispose();
    }

    protected unsafe int? GetMatchingIndex(string[] entries)
    {
        var millisSinceLastEscape = (DateTime.Now - P.EscapeLastPressed).TotalMilliseconds;

        var target = Svc.Targets.Target;
        var targetName = target != null ? target.Name.ExtractText() : string.Empty;

        var nodes = P.Config.GetAllNodes().OfType<ListEntryNode>();
        foreach (var node in nodes)
        {
            if (!node.Enabled || string.IsNullOrEmpty(node.Text))
                continue;

            if (millisSinceLastEscape < 1000 && node == P.LastSelectedListNode && targetName == P.EscapeTargetName)
                continue;

            var (matched, index) = EntryMatchesTexts(node, entries);
            if (!matched)
                continue;

            if (node.TargetRestricted && !string.IsNullOrEmpty(node.TargetText))
            {
                if (!string.IsNullOrEmpty(targetName) && EntryMatchesTargetName(node, targetName))
                {
                    Svc.Log.Debug($"OnSetupSelectListFeature: Matched on {node.Text} ({node.TargetText})");
                    P.LastSelectedListNode = node;
                    return index;
                }
            }
            else
            {
                Svc.Log.Debug($"OnSetupSelectListFeature: Matched on {node.Text}");
                P.LastSelectedListNode = node;
                return index;
            }
        }
        return null;
    }

    protected unsafe void SetupOnItemSelectedHook(PopupMenu* popupMenu)
    {
        if (onItemSelectedHook != null) return;

        var onItemSelectedAddress = (nint)popupMenu->VirtualTable->ReceiveEvent;
        onItemSelectedHook = Svc.Hook.HookFromAddress<AddonReceiveEventDelegate>(onItemSelectedAddress, OnItemSelectedDetour);
        onItemSelectedHook.Enable();
    }

    private unsafe nint OnItemSelectedDetour(AtkEventListener* self, AtkEventType eventType, uint eventParam, AtkEvent* eventData, ulong* inputData)
    {
        Svc.Log.Debug($"PopupMenu RCV: listener={onItemSelectedHook.Address} {(nint)self:X}, type={eventType}, param={eventParam}, input={inputData[0]:X16} {inputData[1]:X16} {inputData[2]:X16} {(int)inputData[2]}");
        try
        {
            var target = Svc.Targets.Target;
            var targetName = P.LastSeenListTarget = target != null ? target.Name.ExtractText() : string.Empty;
            P.LastSeenListSelection = P.LastSeenListEntries[(int)inputData[2]].Text;
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex, "Don't crash the game.");
        }
        return onItemSelectedHook.Original(self, eventType, eventParam, eventData, inputData);
    }

    private unsafe string?[] GetEntryTexts(PopupMenu* popupMenu)
    {
        var count = popupMenu->EntryCount;
        var entryTexts = new string?[count];

        Svc.Log.Debug($"SelectString: Reading {count} strings");
        for (var i = 0; i < count; i++)
        {
            var textPtr = popupMenu->EntryNames[i];
            entryTexts[i] = textPtr != null ? new string((char*)*textPtr) : string.Empty;
        }

        return entryTexts;
    }

    #region Matching

    private static (bool Matched, int Index) EntryMatchesTexts(ListEntryNode node, string?[] texts)
    {
        for (var i = 0; i < texts.Length; i++)
        {
            var text = texts[i];
            if (text == null)
                continue;

            if (EntryMatchesText(node, text))
                return (true, i);
        }

        return (false, -1);
    }

    private static bool EntryMatchesText(ListEntryNode node, string text)
        => node.IsTextRegex && (node.TextRegex?.IsMatch(text) ?? false) || !node.IsTextRegex && text.Contains(node.Text);

    private static bool EntryMatchesTargetName(ListEntryNode node, string targetName)
        => node.TargetIsRegex && (node.TargetRegex?.IsMatch(targetName) ?? false) || !node.TargetIsRegex && targetName.Contains(node.TargetText);

    #endregion
}
