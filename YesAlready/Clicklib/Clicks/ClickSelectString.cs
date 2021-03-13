using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Runtime.InteropServices;

namespace Clicklib.Clicks
{
    internal sealed class ClickSelectString : ClickBase
    {
        protected override string Name => "SelectString";
        protected override string AddonName => "SelectString";

        public unsafe ClickSelectString(DalamudPluginInterface pluginInterface) : base(pluginInterface)
        {
            AvailableClicks["select_string1"] = (addon) => ClickItem(addon, 0);
            AvailableClicks["select_string2"] = (addon) => ClickItem(addon, 1);
            AvailableClicks["select_string3"] = (addon) => ClickItem(addon, 2);
            AvailableClicks["select_string4"] = (addon) => ClickItem(addon, 3);
            AvailableClicks["select_string5"] = (addon) => ClickItem(addon, 4);
            AvailableClicks["select_string6"] = (addon) => ClickItem(addon, 5);
            AvailableClicks["select_string7"] = (addon) => ClickItem(addon, 6);
            AvailableClicks["select_string8"] = (addon) => ClickItem(addon, 7);
            AvailableClicks["select_string9"] = (addon) => ClickItem(addon, 8);
        }

        private unsafe void ClickItem(IntPtr addonPtr, int index)
        {
            var addon = (AddonSelectString*)addonPtr;
            var eventThing = &addon->SelectStringThing;
            var componentList = eventThing->AtkComponentList;

            PluginLog.Information($"Perparing to send select_string{index}");

            var arg5 = Marshal.AllocHGlobal(0x40);
            for (var i = 0; i < 0x40; i++)
                Marshal.WriteByte(arg5, i, 0);

            Marshal.WriteIntPtr(arg5, new IntPtr(componentList->ItemRendererList[index].AtkComponentListItemRenderer));
            Marshal.WriteInt16(arg5, 0x10, (short)index);
            Marshal.WriteInt16(arg5, 0x16, (short)index);

            SendClick(new IntPtr(eventThing), EventType.LIST_INDEX_CHANGE, 0, componentList->AtkComponentBase.OwnerNode, arg5);
        }
    }
}
