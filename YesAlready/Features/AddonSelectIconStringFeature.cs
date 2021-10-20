using System;

using ClickLib.Clicks;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI;
using YesAlready.BaseFeatures;

namespace YesAlready.Features
{
    /// <summary>
    /// AddonSelectString feature.
    /// </summary>
    internal class AddonSelectIconStringFeature : OnSetupSelectListFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddonSelectIconStringFeature"/> class.
        /// </summary>
        public AddonSelectIconStringFeature()
            : base(Service.Address.AddonSelectIconStringOnSetupAddress)
        {
        }

        /// <inheritdoc/>
        protected override string AddonName => "SelectIconString";

        /// <inheritdoc/>
        protected unsafe override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
        {
            var addonPtr = (AddonSelectIconString*)addon;
            var popupMenu = &addonPtr->PopupMenu.PopupMenu;

            this.SetupOnItemSelectedHook(popupMenu);
            this.CompareNodesToEntryTexts(addon, popupMenu);
        }

        /// <inheritdoc/>
        protected override void SelectItemExecute(IntPtr addon, int index)
        {
            PluginLog.Debug($"AddonSelectIconString: Selecting {index}");
            ClickSelectIconString.Using(addon).SelectItem((ushort)index);
        }
    }
}
