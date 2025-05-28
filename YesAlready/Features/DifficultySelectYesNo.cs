using ECommons.EzHookManager;

namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
public class DifficultySelectYesNo : AddonFeature
{
    protected override bool IsEnabled() => C.DifficultySelectYesNoEnabled;

    public delegate nint ExecuteCommandDelegate(int command, int a1 = 0, int a2 = 0, int a3 = 0, int a4 = 0);
    public static readonly ExecuteCommandDelegate? ExecuteCommand = EzDelegate.Get<ExecuteCommandDelegate>("B9 ?? ?? ?? ?? E8 ?? ?? ?? ?? 8D 46 0A");
    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk)
    {
        Log($"Selecting difficulty: {C.DifficultySelectYesNo} [{ExecuteCommand != null}]");
        ExecuteCommand?.Invoke(823, (int)C.DifficultySelectYesNo);
    }

    public enum Difficulty
    {
        Normal = 0,
        Easy = 1,
        VeryEasy = 2,
    }
}
