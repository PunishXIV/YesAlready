using System;
using System.Linq;
using System.Reflection;

namespace YesAlready.BaseFeatures;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AddonFeatureAttribute(AddonEvent eventType, string? addonName = null) : Attribute
{
    /// <summary>
    /// Name of the addon to register the listener for. Will use the class name if one is not provided.
    /// </summary>
    public string? AddonName { get; } = addonName;
    public AddonEvent EventType { get; } = eventType;
}

public abstract class AddonFeature : BaseFeature
{
    private AddonFeatureAttribute[]? _attributes;
    private Func<bool>[]? _enableGetters;

    public BotherAttribute[] Bothers { get; private set; } = [];

    public string[] AddonNames => [.. GetType().GetCustomAttributes<AddonFeatureAttribute>(true).Select(a => a.AddonName ?? GetType().Name).Distinct()];

    public override void Enable()
    {
        base.Enable();
        Bothers = [.. GetType().GetCustomAttributes<BotherAttribute>(true)];
        _enableGetters = [.. Bothers.Where(b => b.ContributesToEnable).Select(CreateGetter)];
        _attributes = [.. GetType().GetCustomAttributes<AddonFeatureAttribute>(true)];

        foreach (var attr in _attributes)
            Svc.AddonLifecycle.RegisterListener(attr.EventType, attr.AddonName ?? GetType().Name, OnAddonEvent);
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
        HandleAddonEvent(eventType, addonInfo);
    }

    protected abstract unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo);

    /// <summary>
    /// Default: true when there are no contributing [Bother]s; otherwise OR of those config bools.
    /// Override and call base for runtime gates.
    /// </summary>
    protected virtual bool IsEnabled()
    {
        if (_enableGetters is not { Length: > 0 })
            return true;

        foreach (var getter in _enableGetters)
        {
            if (getter())
                return true;
        }

        return false;
    }

    protected void Log(string msg) => PluginLog.Debug($"[{GetType().Name}] {msg}");
    protected void LogVerbose(string message) => PluginLog.Verbose($"[{GetType().Name}] {message}");
    protected void LogError(string message) => PluginLog.Error($"[{GetType().Name}] {message}");

    private static Func<bool> CreateGetter(BotherAttribute bother)
    {
        var prop = typeof(Configuration).GetProperty(bother.ConfigProperty, BindingFlags.Instance | BindingFlags.Public)
            ?? throw new InvalidOperationException($"Configuration has no property '{bother.ConfigProperty}' for [Bother] on a feature.");

        if (prop.PropertyType != typeof(bool) || prop.GetMethod is null)
            throw new InvalidOperationException($"Configuration.{bother.ConfigProperty} must be a public bool property.");

        return () => (bool)prop.GetValue(C)!;
    }
}
