using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.UI;
using System;

namespace Clicklib.Clicks
{
    internal sealed class ClickContextIconMenu : ClickBase
    {
        protected override string Name => "ContextIconMenu";
        protected override string AddonName => "ContextIconMenu";

        public unsafe ClickContextIconMenu(DalamudPluginInterface pluginInterface) : base(pluginInterface)
        {
            //AvailableClicks["context_icon_menu1"] = (addon) => ContextIconMenu(addon, 1);
        }

        private unsafe void ContextIconMenu(IntPtr addon, int index)
        {
            var uiAddon = (AddonContextIconMenu*)addon;
            var target = uiAddon->AtkComponentList240->AtkComponentBase.OwnerNode;
            //SendClick(EventType.LIST_ITEM_CLICK, 3, target);
            //SendClick(EventType.LIST_INDEX_CHANGE, 0, target, index);
            //SendClick(EventType.LIST_ITEM_DOUBLE_CLICK, 2, target);
        }
    }
}
