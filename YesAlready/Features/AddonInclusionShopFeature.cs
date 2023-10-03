using System;

using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Component.GUI;

using YesAlready.BaseFeatures;

using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace YesAlready.Features;

/// <summary>
/// AddonInclusionShop feature.
/// </summary>
internal class AddonInclusionShopFeature : OnSetupFeature, IDisposable
{
    [Signature("48 89 5C 24 ?? 57 48 83 EC 20 48 8B DA 4D 8B D0 32 D2", DetourName = nameof(AgentReceiveEventDetour))]
    private readonly Hook<AgentReceiveEventDelegate> agentReceiveEventHook = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddonInclusionShopFeature"/> class.
    /// </summary>
    public AddonInclusionShopFeature()
        : base("85 D2 0F 8E ?? ?? ?? ?? 4C 8B DC 55 53 41 54")
    {
        Service.Hook.InitializeFromAttributes(this);

        this.agentReceiveEventHook.Enable();
    }

    private unsafe delegate IntPtr AgentReceiveEventDelegate(IntPtr agent, IntPtr eventData, AtkValue* values, uint valueCount, ulong eventKind);

    /// <inheritdoc/>
    protected override string AddonName => "InclusionShop";

    /// <inheritdoc/>
    public new void Dispose()
    {
        this.agentReceiveEventHook.Disable();
        this.agentReceiveEventHook.Dispose();
        base.Dispose();
    }

    /// <inheritdoc/>
    protected unsafe override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
    {
        if (!Service.Configuration.InclusionShopRememberEnabled)
            return;

        var unitbase = (AtkUnitBase*)addon;

        PluginLog.Debug($"Firing 12,{Service.Configuration.InclusionShopRememberCategory}");
        using var categoryValues = new AtkValueArray(12, Service.Configuration.InclusionShopRememberCategory);
        unitbase->FireCallback(2, categoryValues);

        PluginLog.Debug($"Firing 13,{Service.Configuration.InclusionShopRememberSubcategory}");
        using var subcategoryValues = new AtkValueArray(13, Service.Configuration.InclusionShopRememberSubcategory);
        unitbase->FireCallback(2, subcategoryValues);
    }

    private unsafe IntPtr AgentReceiveEventDetour(IntPtr agent, IntPtr eventData, AtkValue* values, uint valueCount, ulong eventKind)
    {
        IntPtr Original() => this.agentReceiveEventHook.Original(agent, eventData, values, valueCount, eventKind);

        if (valueCount != 2)
            return Original();

        var atkValue0 = values[0];
        if (atkValue0.Type != ValueType.Int)
            return Original();

        var val0 = atkValue0.Int;
        if (val0 == 12)
        {
            var val1 = values[1].UInt;
            if (val1 != Service.Configuration.InclusionShopRememberCategory)
            {
                PluginLog.Debug($"Remembring InclusionShop category: {val1}");
                Service.Configuration.InclusionShopRememberCategory = val1;
                Service.Configuration.InclusionShopRememberSubcategory = 0;
                Service.Configuration.Save();
            }
        }
        else if (val0 == 13)
        {
            var val1 = values[1].UInt;
            if (val1 != Service.Configuration.InclusionShopRememberSubcategory)
            {
                PluginLog.Debug($"Remembring InclusionShop subcategory: {val1}");
                Service.Configuration.InclusionShopRememberSubcategory = val1;
                Service.Configuration.Save();
            }
        }

        return Original();
    }
}
