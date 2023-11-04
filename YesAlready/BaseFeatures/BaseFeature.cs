using ECommons.DalamudServices;

namespace YesAlready.BaseFeatures;

public abstract class BaseFeature
{
    public virtual bool Enabled { get; protected set; }
    public virtual string Key => GetType().Name;

    public virtual void Enable()
    {
        Svc.Log.Debug($"Enabling {Key}");
        Enabled = true;
    }

    public virtual void Disable()
    {
        Svc.Log.Debug($"Disabling {Key}");
        Enabled = false;
    }
}
