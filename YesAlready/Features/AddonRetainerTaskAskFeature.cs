using System;

using ClickLib.Clicks;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI;
using YesAlready.BaseFeatures;

namespace YesAlready.Features
{
    /// <summary>
    /// AddonRetainerTaskAsk feature.
    /// </summary>
    internal class AddonRetainerTaskAskFeature : OnSetupFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddonRetainerTaskAskFeature"/> class.
        /// </summary>
        public AddonRetainerTaskAskFeature()
            : base(Service.Address.AddonRetainerTaskAskOnSetupAddress)
        {
        }

        /// <inheritdoc/>
        protected override string AddonName => "RetainerTaskAsk";

        /// <inheritdoc/>
        protected unsafe override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
        {
            if (!Service.Configuration.RetainerTaskAskEnabled)
                return;

            //var addonPtr = (AddonRetainerTaskAsk*)addon;
            //var flags = addonPtr->AssignButton->AtkComponentBase.OwnerNode->AtkResNode.Flags;
            //PluginLog.Information($"{flags} {(flags & (1 << 5)) != 0}");

            //if (!addonPtr->AssignButton->IsEnabled)
            //{
            //    PluginLog.Information($"BUTTON NOT ENABLED :(");
            //    return;
            //}

            //PluginLog.Information($"ASSIGN");
            ClickRetainerTaskAsk.Using(addon).Assign();
        }
    }
}
