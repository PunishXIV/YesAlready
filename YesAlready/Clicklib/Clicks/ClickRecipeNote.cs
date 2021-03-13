using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace Clicklib.Clicks
{
    internal sealed class ClickRecipeNote : ClickBase
    {
        protected override string Name => "RecipeBook";
        protected override string AddonName => "RecipeNote";

        public unsafe ClickRecipeNote(DalamudPluginInterface pluginInterface) : base(pluginInterface)
        {
            AvailableClicks["synthesize"] = (addon) => SendClick(addon, EventType.CHANGE, 13, ((AddonRecipeNote*)addon)->SynthesizeButton->AtkComponentBase.OwnerNode);
            AvailableClicks["trial_synthesis"] = (addon) => SendClick(addon, EventType.CHANGE, 15, ((AddonRecipeNote*)addon)->TrialSynthesisButton->AtkComponentBase.OwnerNode);
        }
    }
}
