using System;
using System.Linq;

using ClickLib.Clicks;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI;
using YesAlready.BaseFeatures;

namespace YesAlready.Features
{
    /// <summary>
    /// AddonTalk feature.
    /// </summary>
    internal class AddonTalkFeature : UpdateFeature
    {
        private ClickTalk? clickTalk = null;
        private IntPtr lastTalkAddon = IntPtr.Zero;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddonTalkFeature"/> class.
        /// </summary>
        public AddonTalkFeature()
            : base(Service.Address.AddonTalkUpdateAddress)
        {
        }

        /// <inheritdoc/>
        protected override string AddonName => "Talk";

        /// <inheritdoc/>
        protected unsafe override void UpdateImpl(IntPtr addon, IntPtr a2, IntPtr a3)
        {
            var addonPtr = (AddonTalk*)addon;
            if (!addonPtr->AtkUnitBase.IsVisible)
                return;

            var target = Service.TargetManager.Target;
            var targetName = Service.Plugin.LastSeenTalkTarget = target != null
                ? Service.Plugin.GetSeStringText(target.Name)
                : string.Empty;

            var nodes = Service.Configuration.GetAllNodes().OfType<TalkEntryNode>();
            foreach (var node in nodes)
            {
                if (!node.Enabled || string.IsNullOrEmpty(node.TargetText))
                    continue;

                var matched = this.EntryMatchesTargetName(node, targetName);
                if (!matched)
                    continue;

                if (this.clickTalk == null || this.lastTalkAddon != addon)
                    this.clickTalk = ClickTalk.Using(this.lastTalkAddon = addon);

                PluginLog.Debug("AddonTalk: Advancing");
                this.clickTalk.Click();
                return;
            }
        }

        private bool EntryMatchesTargetName(TalkEntryNode node, string targetName)
        {
            return (node.TargetIsRegex && (node.TargetRegex?.IsMatch(targetName) ?? false)) ||
                  (!node.TargetIsRegex && targetName.Contains(node.TargetText));
        }
    }
}
