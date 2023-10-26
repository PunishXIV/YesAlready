using ECommons.Logging;
using Dalamud.Memory;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;
using Dalamud.Hooking;

namespace ECommons.Automation
{
    public unsafe static class Callback
    {
        const string Sig = "E8 ?? ?? ?? ?? 8B 4C 24 20 0F B6 D8";
        internal delegate byte AtkUnitBase_FireCallbackDelegate(AtkUnitBase* Base, int valueCount, AtkValue* values, byte updateState);
        internal static AtkUnitBase_FireCallbackDelegate FireCallback = null;
        static Hook<AtkUnitBase_FireCallbackDelegate> AtkUnitBase_FireCallbackHook;

        public static readonly AtkValue ZeroAtkValue = new() { Type = 0, Int = 0 };

        internal static void Initialize()
        {
            var ptr = Svc.SigScanner.ScanText(Sig);
            FireCallback = Marshal.GetDelegateForFunctionPointer<AtkUnitBase_FireCallbackDelegate>(ptr);
            PluginLog.Information($"Initialized Callback module, FireCallback = 0x{ptr:X16}");
        }

        public static void InstallHook()
        {
            if (FireCallback == null) Initialize();
            AtkUnitBase_FireCallbackHook ??= Svc.Hook.HookFromSignature<AtkUnitBase_FireCallbackDelegate>(Sig, AtkUnitBase_FireCallbackDetour);
            if (AtkUnitBase_FireCallbackHook.IsEnabled)
            {
                PluginLog.Error("AtkUnitBase_FireCallbackHook is already enabled");
            }
            else
            {
                AtkUnitBase_FireCallbackHook.Enable();
                PluginLog.Information("AtkUnitBase_FireCallbackHook enabled");
            }
        }

        public static void UninstallHook()
        {
            if (FireCallback == null)
            {
                PluginLog.Error("AtkUnitBase_FireCallbackHook not initialized yet");
            }
            if (!AtkUnitBase_FireCallbackHook.IsEnabled)
            {
                PluginLog.Error("AtkUnitBase_FireCallbackHook is already disabled");
            }
            else
            {
                AtkUnitBase_FireCallbackHook.Disable();
                PluginLog.Information("AtkUnitBase_FireCallbackHook disabled");
            }
        }

        static byte AtkUnitBase_FireCallbackDetour(AtkUnitBase* Base, int valueCount, AtkValue* values, byte updateState)
        {
            var ret = AtkUnitBase_FireCallbackHook?.Original(Base, valueCount, values, updateState);
            try
            {
                PluginLog.Debug($"Callback on {MemoryHelper.ReadStringNullTerminated((nint)Base->Name)}, valueCount={valueCount}, updateState={updateState}\n{DecodeValues(valueCount, values).Select(x => $"    {x}").Join("\n")}");
            }
            catch(Exception e)
            {
                e.Log();
            }
            return ret ?? 0;
        }

        public static void FireRaw(AtkUnitBase* Base, int valueCount, AtkValue* values, byte updateState = 0)
        {
            if (FireCallback == null) Initialize();
            FireCallback(Base, valueCount, values, updateState);
        }
        
        public static void Fire(AtkUnitBase* Base, bool updateState, params object[] values)
        {
            if (Base == null) throw new Exception("Null UnitBase");
            var atkValues = (AtkValue*)Marshal.AllocHGlobal(values.Length * sizeof(AtkValue));
            if (atkValues == null) return;
            try
            {
                for (var i = 0; i < values.Length; i++)
                {
                    var v = values[i];
                    switch (v)
                    {
                        case uint uintValue:
                            atkValues[i].Type = ValueType.UInt;
                            atkValues[i].UInt = uintValue;
                            break;
                        case int intValue:
                            atkValues[i].Type = ValueType.Int;
                            atkValues[i].Int = intValue;
                            break;
                        case float floatValue:
                            atkValues[i].Type = ValueType.Float;
                            atkValues[i].Float = floatValue;
                            break;
                        case bool boolValue:
                            atkValues[i].Type = ValueType.Bool;
                            atkValues[i].Byte = (byte)(boolValue ? 1 : 0);
                            break;
                        case string stringValue:
                            {
                                atkValues[i].Type = ValueType.String;
                                var stringBytes = Encoding.UTF8.GetBytes(stringValue);
                                var stringAlloc = Marshal.AllocHGlobal(stringBytes.Length + 1);
                                Marshal.Copy(stringBytes, 0, stringAlloc, stringBytes.Length);
                                Marshal.WriteByte(stringAlloc, stringBytes.Length, 0);
                                atkValues[i].String = (byte*)stringAlloc;
                                break;
                            }
                        case AtkValue rawValue:
                            {
                                atkValues[i] = rawValue;
                                break;
                            }
                        default:
                            throw new ArgumentException($"Unable to convert type {v.GetType()} to AtkValue");
                    }
                }
                List<string> CallbackValues = new();
                for(var i = 0; i < values.Length; i++)
                {
                    CallbackValues.Add($"    Value {i}: [input: {values[i]}/{values[i]?.GetType().Name}] -> {DecodeValue(atkValues[i])})");
                }
                PluginLog.Verbose($"Firing callback: {MemoryHelper.ReadStringNullTerminated((nint)Base->Name)}, valueCount = {values.Length}, updateStatte = {updateState}, values:\n");
                FireRaw(Base, values.Length, atkValues, (byte)(updateState ?1:0));
            }
            finally
            {
                for (var i = 0; i < values.Length; i++)
                {
                    if (atkValues[i].Type == ValueType.String)
                    {
                        Marshal.FreeHGlobal(new IntPtr(atkValues[i].String));
                    }
                }
                Marshal.FreeHGlobal(new IntPtr(atkValues));
            }
        }

        public static List<string> DecodeValues(int cnt, AtkValue* values)
        {
            var atkValueList = new List<string>();
            try
            {
                for (var i = 0; i < cnt; i++)
                {
                    atkValueList.Add(DecodeValue(values[i]));
                }
            }
            catch (Exception e)
            {
                e.Log();
            }
            return atkValueList;
        }

        public static string DecodeValue(AtkValue a)
        {
            var str = new StringBuilder(a.Type.ToString()).Append(": ");
            switch (a.Type)
            {
                case ValueType.Int:
                    {
                        str.Append(a.Int);
                        break;
                    }
                case ValueType.String:
                    {
                        str.Append(Marshal.PtrToStringUTF8(new IntPtr(a.String)));
                        break;
                    }
                case ValueType.UInt:
                    {
                        str.Append(a.UInt);
                        break;
                    }
                case ValueType.Bool:
                    {
                        str.Append(a.Byte != 0);
                        break;
                    }
                default:
                    {
                        str.Append($"Unknown Type: {a.Int}");
                        break;
                    }
            }
            return str.ToString();
        }

        internal static void Dispose()
        {
            AtkUnitBase_FireCallbackHook?.Dispose();
            AtkUnitBase_FireCallbackHook = null;
            FireCallback = null;
        }
    }
}
