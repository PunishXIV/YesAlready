using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Linq;

namespace ECommons.PartyFunctions;

public class UniversalPartyMember
{
    public String Name { get; init; }
    public ExcelResolver<World> HomeWorld { get; init; }
    public ExcelResolver<World> CurrentWorld { get; init; }

    internal GameObject GameObjectInternal = null;
    public GameObject GameObject
    {
        get
        {
            if (UniversalParty.IsCrossWorldParty)
            {
                return Svc.Objects.FirstOrDefault(x => x is PlayerCharacter pc && pc.HomeWorld.Id == this.HomeWorld.Id && x.Name.ToString() == this.Name);
            }
            else
            {
                return GameObjectInternal;
            }
        }
    }
}
