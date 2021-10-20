using System;
using System.Linq;

using Dalamud.Game;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Hooking;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace YesAlready.BaseFeatures
{
    /// <summary>
    /// An abstract that hooks OnItemSelected and provides a list selection feature.
    /// </summary>
    internal abstract class OnSetupSelectListFeature : OnSetupFeature, IDisposable
    {
        private Hook<OnItemSelectedDelegate>? onItemSelectedHook = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="OnSetupSelectListFeature"/> class.
        /// </summary>
        /// <param name="onSetupAddress">Address to the OnSetup method.</param>
        protected OnSetupSelectListFeature(IntPtr onSetupAddress)
            : base(onSetupAddress)
        {
        }

        /// <summary>
        /// A delegate matching PopupMenu.OnItemSelected.
        /// </summary>
        /// <param name="popupMenu">PopupMenu address.</param>
        /// <param name="index">Selected index.</param>
        /// <param name="a3">Parameter 3.</param>
        /// <param name="a4">Parameter 4.</param>
        /// <returns>Unknown.</returns>
        private delegate byte OnItemSelectedDelegate(IntPtr popupMenu, uint index, IntPtr a3, IntPtr a4);

        /// <inheritdoc/>
        public new void Dispose()
        {
            this.onItemSelectedHook?.Disable();
            this.onItemSelectedHook?.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// Compare the configuration nodes to the given texts and execute if any match.
        /// </summary>
        /// <param name="addon">Addon to click.</param>
        /// <param name="popupMenu">PopupMenu to match text on.</param>
        protected unsafe void CompareNodesToEntryTexts(IntPtr addon, PopupMenu* popupMenu)
        {
            var millisSinceLastEscape = (DateTime.Now - Service.Plugin.EscapeLastPressed).TotalMilliseconds;
            if (millisSinceLastEscape < 1000)
                return;

            var target = Service.TargetManager.Target;
            var targetName = target != null
                ? Service.Plugin.GetSeStringText(target.Name)
                : string.Empty;

            var texts = this.GetEntryTexts(popupMenu);
            var nodes = Service.Configuration.GetAllNodes().OfType<ListEntryNode>();
            foreach (var node in nodes)
            {
                if (!node.Enabled || string.IsNullOrEmpty(node.Text))
                    continue;

                var (matched, index) = this.EntryMatchesTexts(node, texts);
                if (!matched)
                    continue;

                if (node.TargetRestricted && !string.IsNullOrEmpty(node.TargetText))
                {
                    if (!string.IsNullOrEmpty(targetName) && this.EntryMatchesTargetName(node, targetName))
                    {
                        PluginLog.Debug($"OnSetupSelectListFeature: Matched on {node.Text} ({node.TargetText})");
                        this.SelectItemExecute(addon, index);
                        return;
                    }
                }
                else
                {
                    PluginLog.Debug($"OnSetupSelectListFeature: Matched on {node.Text}");
                    this.SelectItemExecute(addon, index);
                    return;
                }
            }
        }

        /// <summary>
        /// Execute a list selection click with the given addon and index.
        /// </summary>
        /// <param name="addon">Addon to click.</param>
        /// <param name="index">Selection index.</param>
        protected abstract void SelectItemExecute(IntPtr addon, int index);

        /// <summary>
        /// Setup a PopupMenu OnItemSelected hook if it has not already been.
        /// </summary>
        /// <param name="popupMenu">Pointer to the popupMenu.</param>
        protected unsafe void SetupOnItemSelectedHook(PopupMenu* popupMenu)
        {
            if (this.onItemSelectedHook != null)
                return;

            var onItemSelectedAddress = (IntPtr)popupMenu->AtkEventListener.vfunc[3];
            this.onItemSelectedHook = new Hook<OnItemSelectedDelegate>(onItemSelectedAddress, this.OnItemSelectedDetour);
            this.onItemSelectedHook.Enable();
        }

        private unsafe byte OnItemSelectedDetour(IntPtr popupMenu, uint index, IntPtr a3, IntPtr a4)
        {
            try
            {
                var popupMenuPtr = (PopupMenu*)popupMenu;
                if (index < popupMenuPtr->EntryCount)
                {
                    var entryPtr = popupMenuPtr->EntryNames[index];
                    var entryText = Service.Plugin.LastSeenListSelection = entryPtr != null
                        ? Service.Plugin.GetSeStringText(entryPtr)
                        : string.Empty;

                    var target = Service.TargetManager.Target;
                    var targetName = Service.Plugin.LastSeenListTarget = target != null
                        ? Service.Plugin.GetSeStringText(target.Name)
                        : string.Empty;

                    PluginLog.Debug($"ItemSelected: target={targetName} text={entryText}");
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Don't crash the game");
            }

            return this.onItemSelectedHook!.Original(popupMenu, index, a3, a4);
        }

        private unsafe string?[] GetEntryTexts(PopupMenu* popupMenu)
        {
            var count = popupMenu->EntryCount;
            var entryTexts = new string?[count];

            PluginLog.Debug($"SelectString: Reading {count} strings");
            for (var i = 0; i < count; i++)
            {
                var textPtr = popupMenu->EntryNames[i];
                entryTexts[i] = textPtr != null
                    ? Service.Plugin.GetSeStringText(textPtr)
                    : null;
            }

            return entryTexts;
        }

        #region Matching

        private (bool Matched, int Index) EntryMatchesTexts(ListEntryNode node, string?[] texts)
        {
            for (var i = 0; i < texts.Length; i++)
            {
                var text = texts[i];
                if (text == null)
                    continue;

                if (this.EntryMatchesText(node, text))
                    return (true, i);
            }

            return (false, -1);
        }

        private bool EntryMatchesText(ListEntryNode node, string text)
        {
            return (node.IsTextRegex && (node.TextRegex?.IsMatch(text) ?? false)) ||
                  (!node.IsTextRegex && text.Contains(node.Text));
        }

        private bool EntryMatchesTargetName(ListEntryNode node, string targetName)
        {
            return (node.TargetIsRegex && (node.TargetRegex?.IsMatch(targetName) ?? false)) ||
                  (!node.TargetIsRegex && targetName.Contains(node.TargetText));
        }

        #endregion
    }
}
