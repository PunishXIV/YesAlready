using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using System;

namespace ECommons.GameFunctions;

public unsafe static class PlayerFunctions
{
    public static bool TryGetPlaceholder(this GameObject pc, out int number, bool verbose = false)
    {
        for(var i = 1; i <= 8; i++)
        {
            var optr = Framework.Instance()->GetUiModule()->GetPronounModule()->ResolvePlaceholder($"<{i}>", 0, 0);
            if(verbose) PluginLog.Debug($"Placeholder {i} value {(optr == null ? "null" : optr->ObjectID)}");
            if (pc.Address == (IntPtr)optr)
            {
                number = i;
                return true;
            }
        }
        number = default;
        return false;
    }

    public static FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara* BattleChara(this PlayerCharacter o)
    {
        return (FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara*)o.Address;
    }

    public static FFXIVClientStructs.FFXIV.Client.Game.Character.Character* Character(this PlayerCharacter o)
    {
        return (FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)o.Address;
    }

    public static FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject* GameObject(this PlayerCharacter o)
    {
        return (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)o.Address;
    }
}
