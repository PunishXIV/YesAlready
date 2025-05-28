using System;
using ECommons.EzHookManager;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class InclusionShop : AddonFeature
{
    protected override bool IsEnabled() => C.InclusionShopRememberEnabled;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk)
    {
        if (!GenericHelpers.IsAddonReady(atk)) return;

        PluginLog.Debug($"Firing 12,{C.InclusionShopRememberCategory}");
        using var categoryValues = new AtkValueArray(12, C.InclusionShopRememberCategory);
        atk->FireCallback(2, categoryValues);

        PluginLog.Debug($"Firing 13,{C.InclusionShopRememberSubcategory}");
        using var subcategoryValues = new AtkValueArray(13, C.InclusionShopRememberSubcategory);
        atk->FireCallback(2, subcategoryValues);
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
            if (val1 != C.InclusionShopRememberCategory)
            {
                PluginLog.Debug($"Remembring InclusionShop category: {val1}");
                C.InclusionShopRememberCategory = val1;
                C.InclusionShopRememberSubcategory = 0;
                C.Save();
            }
        }
        else if (val0 == 13)
        {
            var val1 = values[1].UInt;
            if (val1 != C.InclusionShopRememberSubcategory)
            {
                PluginLog.Debug($"Remembring InclusionShop subcategory: {val1}");
                C.InclusionShopRememberSubcategory = val1;
                C.Save();
            }
        }

        return Original();
    }
}
