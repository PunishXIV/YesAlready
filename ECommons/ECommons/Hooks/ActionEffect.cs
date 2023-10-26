using Dalamud.Hooking;
using ECommons.Logging;
using ECommons.DalamudServices;
using ECommons.Hooks.ActionEffectTypes;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ECommons.Hooks
{
    public static unsafe class ActionEffect
    {
        const string Sig = "40 55 53 57 41 54 41 55 41 56 41 57 48 8D AC 24 ?? ?? ?? ?? 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 45 70";

        public delegate void ProcessActionEffect(uint sourceId, Character* sourceCharacter, Vector3* pos, EffectHeader* effectHeader, EffectEntry* effectArray, ulong* effectTail);
        internal static Hook<ProcessActionEffect> ProcessActionEffectHook = null;

        public delegate void ActionEffectCallback(ActionEffectSet set);

        static event ActionEffectCallback _actionEffectEvent;
        public static event ActionEffectCallback ActionEffectEvent
        {
            add
            {
                Hook();
                _actionEffectEvent += value;
            }
            remove => _actionEffectEvent -= value;
        }

        static event Action<uint, ushort, ActionEffectType, uint, ulong, uint> _actionEffectEntryEvent;
        public static event Action<uint, ushort, ActionEffectType, uint, ulong, uint> ActionEffectEntryEvent
        {
            add
            {
                Hook();
                _actionEffectEntryEvent += value;
            }
            remove => _actionEffectEntryEvent -= value;
        }

        static bool doLogging = false;

        private static void Hook()
        {
            if (ProcessActionEffectHook == null)
            {
                if (Svc.SigScanner.TryScanText(Sig, out var ptr))
                {
                    ProcessActionEffectHook = Svc.Hook.HookFromAddress<ProcessActionEffect>(ptr, ProcessActionEffectDetour);
                    Enable();
                    PluginLog.Information($"Requested Action Effect hook and successfully initialized");
                }
                else
                {
                    PluginLog.Error($"Could not find ActionEffect signature");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fullParamsCallback">uint ActionID, ushort animationID, ActionEffectType type, uint sourceID, ulong targetOID, uint damage</param>
        /// <param name="logging"></param>
        /// <exception cref="Exception"></exception>
        [Obsolete($"Please use {nameof(ActionEffectEntryEvent)} instead.")]
        public static void Init(Action<uint, ushort, ActionEffectType, uint, ulong, uint> fullParamsCallback, bool logging = false)
        {
            ActionEffectEntryEvent += fullParamsCallback;
        }

        public static void Enable()
        {
            if (ProcessActionEffectHook?.IsEnabled == false) ProcessActionEffectHook?.Enable();
        }

        public static void Disable()
        {
            if (ProcessActionEffectHook?.IsEnabled == true) ProcessActionEffectHook?.Disable();
        }

        internal static void Dispose()
        {
            if (ProcessActionEffectHook != null)
            {
                PluginLog.Information($"Disposing Action Effect Hook");
                Disable();
                ProcessActionEffectHook?.Dispose();
                ProcessActionEffectHook = null;
            }
        }

        internal static void ProcessActionEffectDetour(uint sourceID, Character* sourceCharacter, Vector3* pos, EffectHeader* effectHeader, EffectEntry* effectArray, ulong* effectTail)
        {
            try
            {
                if(doLogging) PluginLog.Verbose($"--- source actor: {sourceCharacter->GameObject.ObjectID}, action id {effectHeader->ActionID}, anim id {effectHeader->AnimationId} numTargets: {effectHeader->TargetCount} ---");

                // TODO: Reimplement opcode logging, if it's even useful. Original code follows
                // ushort op = *((ushort*) effectHeader.ToPointer() - 0x7);
                // DebugLog(Effect, $"--- source actor: {sourceId}, action id {id}, anim id {animId}, opcode: {op:X} numTargets: {targetCount} ---");

                var set = new ActionEffectSet(sourceID, sourceCharacter, pos, effectHeader, effectArray, effectTail);
                _actionEffectEvent?.Invoke(set);

                foreach( var effect in set.TargetEffects)
                {
                    effect.ForEach(entry =>
                    {
                        if (entry.type == ActionEffectType.Nothing) return;
                        _actionEffectEntryEvent?.Invoke(effectHeader->ActionID, effectHeader->AnimationId, entry.type, sourceID, effect.TargetID, entry.Damage);
                    });
                }
            }
            catch (Exception e)
            {
                PluginLog.Error($"An error has occurred in Action Effect hook.\n{e}");
            }

            ProcessActionEffectHook.Original(sourceID, sourceCharacter, pos, effectHeader, effectArray, effectTail);
        }
    }
}
