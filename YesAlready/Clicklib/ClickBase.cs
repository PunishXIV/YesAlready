using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Clicklib
{
    internal abstract class ClickBase
    {
        protected abstract string Name { get; }
        protected abstract string AddonName { get; }

        protected DalamudPluginInterface Interface { get; private set; }

        protected Dictionary<string, Action<IntPtr>> AvailableClicks { get; } = new Dictionary<string, Action<IntPtr>>();

        protected delegate void ReceiveEventDelegate(IntPtr addon, EventType evt, uint a3, IntPtr a4, IntPtr a5);

        internal ClickBase(DalamudPluginInterface pluginInterface)
        {
            Interface = pluginInterface;
        }

        internal bool Click(string name)
        {
            if (AvailableClicks.TryGetValue(name, out Action<IntPtr> clickDelegate))
            {
                var addon = GetAddonByName(AddonName);
                clickDelegate(addon);
                return true;
            }
            return false;
        }

        protected unsafe void SendClick(IntPtr arg1, EventType arg2, uint arg3, void* target) => SendClick(arg1, arg2, arg3, target, IntPtr.Zero);

        protected unsafe void SendClick(IntPtr arg1, EventType arg2, uint arg3, void* target, IntPtr arg5)
        {
            var receiveEvent = GetReceiveEventDelegate((AtkEventListener*)arg1);

            var arg4 = Marshal.AllocHGlobal(0x40);
            for (var i = 0; i < 0x40; i++)
                Marshal.WriteByte(arg4, i, 0);

            Marshal.WriteIntPtr(arg4, 0x8, new IntPtr(target));
            Marshal.WriteIntPtr(arg4, 0x10, arg1);

            if (arg5 == IntPtr.Zero)
            {
                arg5 = Marshal.AllocHGlobal(0x40);
                for (var i = 0; i < 0x40; i++)
                    Marshal.WriteByte(arg5, i, 0);
            }

            receiveEvent(arg1, arg2, arg3, arg4, arg5);

            Marshal.FreeHGlobal(arg4);
            Marshal.FreeHGlobal(arg5);
        }

        protected IntPtr GetAddonByName(string name) => GetAddonByName(name, 1);

        protected IntPtr GetAddonByName(string name, int index)
        {
            var addon = Interface.Framework.Gui.GetUiObjectByName(name, index);
            if (addon == IntPtr.Zero)
                throw new InvalidClickException($"Window is not available for that click");
            return addon;
        }

        protected unsafe ReceiveEventDelegate GetReceiveEventDelegate(AtkEventListener* eventListener)
        {
            var receiveEventAddress = new IntPtr(eventListener->vfunc[2]);
            return Marshal.GetDelegateForFunctionPointer<ReceiveEventDelegate>(receiveEventAddress);
        }

        protected unsafe (float, float) BacktrackNodePoint(AtkResNode* node)
        {
            if (node == null)
                throw new Exception("Node does not exist");

            float x = node->X + (node->Width / 2);
            float y = node->Y + (node->Height / 2);

            AtkResNode* parent = node;
            while ((parent = parent->ParentNode) != null)
            {
                x += parent->X;
                y += parent->Y;
            }
            return (x, y);
        }

        protected unsafe (float, float) ConvertToAbsolute(float x, float y)
        {
            var ax = x * 0xffff / GetSystemMetrics(SM_CXSCREEN);
            var ay = y * 0xffff / GetSystemMetrics(SM_CYSCREEN);
            return (ax, ay);
        }

        private const int SM_CXSCREEN = 0x0;
        private const int SM_CYSCREEN = 0x1;

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int smIndex);
    }
}
