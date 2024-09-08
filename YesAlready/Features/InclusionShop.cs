using System;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons.EzHookManager;
using FFXIVClientStructs.FFXIV.Component.GUI;
using YesAlready.BaseFeatures;
using YesAlready.Utils;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace YesAlready.Features;

internal class InclusionShop : BaseFeature
{
    public override void Enable()
    {
        base.Enable();
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "InclusionShop", AddonSetup);
    }

    public override void Disable()
    {
        base.Disable();
        Svc.AddonLifecycle.UnregisterListener(AddonSetup);
    }

    protected unsafe void AddonSetup(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active || !P.Config.InclusionShopRememberEnabled) return;

        var addon = addonInfo.Base();
        Svc.Log.Debug($"Firing 12,{P.Config.InclusionShopRememberCategory}");
        using var categoryValues = new AtkValueArray(12, P.Config.InclusionShopRememberCategory);
        addon->FireCallback(2, categoryValues);

        Svc.Log.Debug($"Firing 13,{P.Config.InclusionShopRememberSubcategory}");
        using var subcategoryValues = new AtkValueArray(13, P.Config.InclusionShopRememberSubcategory);
        addon->FireCallback(2, subcategoryValues);
    }

    [EzHook("40 53 48 83 EC ?? 48 8B DA 4D 8B D0", detourName: nameof(AgentReceiveEventDetour), true)]
    private readonly EzHook<AgentReceiveEventDelegate> agentReceiveEventHook = null!;
    private unsafe delegate IntPtr AgentReceiveEventDelegate(IntPtr agent, IntPtr eventData, AtkValue* values, uint valueCount, ulong eventKind);

    public InclusionShop()
    {
        EzSignatureHelper.Initialize(this);
    }

    private unsafe IntPtr AgentReceiveEventDetour(IntPtr agent, IntPtr eventData, AtkValue* values, uint valueCount, ulong eventKind)
    {
        IntPtr Original() => agentReceiveEventHook.Original(agent, eventData, values, valueCount, eventKind);

        if (valueCount != 2)
            return Original();

        var atkValue0 = values[0];
        if (atkValue0.Type != ValueType.Int)
            return Original();

        var val0 = atkValue0.Int;
        if (val0 == 12)
        {
            var val1 = values[1].UInt;
            if (val1 != P.Config.InclusionShopRememberCategory)
            {
                Svc.Log.Debug($"Remembring InclusionShop category: {val1}");
                P.Config.InclusionShopRememberCategory = val1;
                P.Config.InclusionShopRememberSubcategory = 0;
                P.Config.Save();
            }
        }
        else if (val0 == 13)
        {
            var val1 = values[1].UInt;
            if (val1 != P.Config.InclusionShopRememberSubcategory)
            {
                Svc.Log.Debug($"Remembring InclusionShop subcategory: {val1}");
                P.Config.InclusionShopRememberSubcategory = val1;
                P.Config.Save();
            }
        }

        return Original();
    }
}
