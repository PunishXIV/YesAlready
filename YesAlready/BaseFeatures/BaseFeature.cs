namespace YesAlready.BaseFeatures;

public abstract class BaseFeature
{
    public virtual bool Enabled { get; protected set; }
    public virtual string Key => GetType().Name;

    public virtual void Enable()
    {
        PluginLog.Debug($"Enabling {Key}");
        Enabled = true;
    }

    public virtual void Disable()
    {
        PluginLog.Debug($"Disabling {Key}");
        Enabled = false;
    }
}
