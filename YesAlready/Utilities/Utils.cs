using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace YesAlready.Utilities;
public static class Utils
{
    public static unsafe AtkUnitBase* Base(this AddonArgs args) => (AtkUnitBase*)args.Addon;
}
