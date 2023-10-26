using System;
using System.Linq;

using Dalamud.Hooking;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace YesAlready.BaseFeatures;

internal abstract class OnSetupSelectListFeature : BaseFeature, IDisposable
{
    private Hook<OnItemSelectedDelegate>? onItemSelectedHook = null;

    protected OnSetupSelectListFeature() { }

    private delegate byte OnItemSelectedDelegate(IntPtr popupMenu, uint index, IntPtr a3, IntPtr a4);

    public void Dispose()
    {
        this.onItemSelectedHook?.Disable();
        this.onItemSelectedHook?.Dispose();
    }

    protected unsafe void CompareNodesToEntryTexts(IntPtr addon, PopupMenu* popupMenu)
    {
        var millisSinceLastEscape = (DateTime.Now - P.EscapeLastPressed).TotalMilliseconds;

        var target = Svc.Targets.Target;
        var targetName = target != null
            ? Utils.SEString.GetSeStringText(target.Name)
            : string.Empty;

        var texts = this.GetEntryTexts(popupMenu);
        var nodes = P.Config.GetAllNodes().OfType<ListEntryNode>();
        foreach (var node in nodes)
        {
            if (!node.Enabled || string.IsNullOrEmpty(node.Text))
                continue;

            if (millisSinceLastEscape < 1000 && node == P.LastSelectedListNode && targetName == P.EscapeTargetName)
                continue;

            var (matched, index) = EntryMatchesTexts(node, texts);
            if (!matched)
                continue;

            if (node.TargetRestricted && !string.IsNullOrEmpty(node.TargetText))
            {
                if (!string.IsNullOrEmpty(targetName) && EntryMatchesTargetName(node, targetName))
                {
                    Svc.Log.Debug($"OnSetupSelectListFeature: Matched on {node.Text} ({node.TargetText})");
                    P.LastSelectedListNode = node;
                    this.SelectItemExecute(addon, index);
                    return;
                }
            }
            else
            {
                Svc.Log.Debug($"OnSetupSelectListFeature: Matched on {node.Text}");
                P.LastSelectedListNode = node;
                this.SelectItemExecute(addon, index);
                return;
            }
        }
    }

    protected abstract void SelectItemExecute(IntPtr addon, int index);

    protected unsafe void SetupOnItemSelectedHook(PopupMenu* popupMenu)
    {
        if (this.onItemSelectedHook != null)
            return;

        var onItemSelectedAddress = (IntPtr)popupMenu->AtkEventListener.vfunc[3];
        this.onItemSelectedHook = Svc.Hook.HookFromAddress<OnItemSelectedDelegate>(onItemSelectedAddress, this.OnItemSelectedDetour);
        this.onItemSelectedHook.Enable();
    }

    private unsafe byte OnItemSelectedDetour(IntPtr popupMenu, uint index, IntPtr a3, IntPtr a4)
    {
        if (popupMenu == IntPtr.Zero)
            return this.onItemSelectedHook!.Original(popupMenu, index, a3, a4);

        try
        {
            var popupMenuPtr = (PopupMenu*)popupMenu;
            if (index < popupMenuPtr->EntryCount)
            {
                var entryPtr = popupMenuPtr->EntryNames[index];
                var entryText = P.LastSeenListSelection = entryPtr != null
                    ? Utils.SEString.GetSeStringText(entryPtr)
                    : string.Empty;

                var target = Svc.Targets.Target;
                var targetName = P.LastSeenListTarget = target != null
                    ? Utils.SEString.GetSeStringText(target.Name)
                    : string.Empty;

                Svc.Log.Debug($"ItemSelected: target={targetName} text={entryText}");
            }
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex, "Don't crash the game");
        }

        return this.onItemSelectedHook!.Original(popupMenu, index, a3, a4);
    }

    private unsafe string?[] GetEntryTexts(PopupMenu* popupMenu)
    {
        var count = popupMenu->EntryCount;
        var entryTexts = new string?[count];

        Svc.Log.Debug($"SelectString: Reading {count} strings");
        for (var i = 0; i < count; i++)
        {
            var textPtr = popupMenu->EntryNames[i];
            entryTexts[i] = textPtr != null
                ? Utils.SEString.GetSeStringText(textPtr)
                : null;
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
    {
        return (node.IsTextRegex && (node.TextRegex?.IsMatch(text) ?? false)) ||
              (!node.IsTextRegex && text.Contains(node.Text));
    }

    private static bool EntryMatchesTargetName(ListEntryNode node, string targetName)
    {
        return (node.TargetIsRegex && (node.TargetRegex?.IsMatch(targetName) ?? false)) ||
              (!node.TargetIsRegex && targetName.Contains(node.TargetText));
    }

    #endregion
}
