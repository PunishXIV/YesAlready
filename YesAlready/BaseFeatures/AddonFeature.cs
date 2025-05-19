using System;
using System.Linq;

namespace YesAlready.BaseFeatures;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AddonFeatureAttribute : Attribute
{
    public string AddonName { get; }
    public AddonEvent EventType { get; }

    public AddonFeatureAttribute(AddonEvent eventType, string? addonName = null)
    {
        EventType = eventType;
        AddonName = addonName ?? GetType().DeclaringType?.Name ?? throw new InvalidOperationException("Could not determine addon name");
    }
}

public abstract class AddonFeature : BaseFeature
{
    private AddonFeatureAttribute[]? _attributes;

    public override void Enable()
    {
        base.Enable();
        _attributes = [.. GetType().GetCustomAttributes(typeof(AddonFeatureAttribute), true).Cast<AddonFeatureAttribute>()];

        if (_attributes != null)
            foreach (var attr in _attributes)
                Svc.AddonLifecycle.RegisterListener(attr.EventType, attr.AddonName, OnAddonEvent);
    }

    public override void Disable()
    {
        base.Disable();
        if (_attributes != null)
            foreach (var attr in _attributes)
                Svc.AddonLifecycle.UnregisterListener(OnAddonEvent);
    }

    protected virtual unsafe void OnAddonEvent(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active || !IsEnabled()) return;
        HandleAddonEvent(eventType, addonInfo, addonInfo.Base());
    }

    protected abstract unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk);
    protected abstract bool IsEnabled();
}
