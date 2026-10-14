using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.Interop;
using System;

namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
[Bother(nameof(Configuration.RequestFill), BotherCategory.Other, "Automatically fill requested items.")]
internal unsafe class Request : AddonFeature
{
    protected override void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo)
    {
        var npcTrade = &UIState.Instance()->NpcTrade;
        var agent = AgentNpcTrade.Instance();
        if (agent == null || !agent->IsAgentActive())
        {
            Log($"AgentNpcTrade null or inactive");
            return;
        }

        if (npcTrade->Requests.Count == 0 || !npcTrade->CanSatisfyRequests())
        {
            Log($"No requests or cannot satisfy requests");
            return;
        }

        var res = new AtkValue();
        Span<AtkValue> param = stackalloc AtkValue[4];

        for (var slot = 0; slot < npcTrade->Requests.Count; slot++)
        {
            if (agent->SelectedTurnInSlot >= 0)
                return; // mid-select

            agent->SelectTurnInSlot((ushort)slot);
            if (agent->SelectedTurnInSlot != slot || agent->SelectedTurnInSlotItemOptions <= 0)
                return;

            param[0].SetInt(0); // confirm
            param[1].SetInt(0); // option 0 (NQ/HQ pick - first matching)
            agent->ReceiveEvent(&res, param.GetPointer(0), 4, 1);
        }

        // commit
        param[0].SetInt(0);
        var addonId = agent->AddonId;
        agent->ReceiveEvent(&res, param.GetPointer(0), 4, 0);
        var addon = RaptureAtkUnitManager.Instance()->GetAddonById((ushort)addonId);
        if (addon != null && addon->IsVisible)
            addon->Close(false);
    }
}
