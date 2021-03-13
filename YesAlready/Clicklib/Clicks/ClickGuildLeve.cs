using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace Clicklib.Clicks
{
    internal sealed class ClickGuildLeve : ClickBase
    {
        protected override string Name => "GuildLeve";
        protected override string AddonName => "GuildLeve";

        public unsafe ClickGuildLeve(DalamudPluginInterface pluginInterface) : base(pluginInterface)
        {
            AvailableClicks["guild_leve_fieldcraft"] = (addon) => SendClick(addon, EventType.CHANGE, 6, ((AddonGuildLeve*)addon)->FieldcraftButton->AtkComponentBase.OwnerNode);
            AvailableClicks["guild_leve_tradecraft"] = (addon) => SendClick(addon, EventType.CHANGE, 7, ((AddonGuildLeve*)addon)->TradecraftButton->AtkComponentBase.OwnerNode);

            AvailableClicks["guild_leve_carpenter"] = (addon) => SendClick(addon, EventType.CHANGE, 9, ((AddonGuildLeve*)addon)->CarpenterButton->AtkComponentBase.OwnerNode);
            AvailableClicks["guild_leve_blacksmith"] = (addon) => SendClick(addon, EventType.CHANGE, 10, ((AddonGuildLeve*)addon)->BlacksmithButton->AtkComponentBase.OwnerNode);
            AvailableClicks["guild_leve_armorer"] = (addon) => SendClick(addon, EventType.CHANGE, 11, ((AddonGuildLeve*)addon)->ArmorerButton->AtkComponentBase.OwnerNode);
            AvailableClicks["guild_leve_goldsmith"] = (addon) => SendClick(addon, EventType.CHANGE, 12, ((AddonGuildLeve*)addon)->GoldsmithButton->AtkComponentBase.OwnerNode);
            AvailableClicks["guild_leve_leatherworker"] = (addon) => SendClick(addon, EventType.CHANGE, 13, ((AddonGuildLeve*)addon)->LeatherworkerButton->AtkComponentBase.OwnerNode);
            AvailableClicks["guild_leve_weaver"] = (addon) => SendClick(addon, EventType.CHANGE, 14, ((AddonGuildLeve*)addon)->WeaverButton->AtkComponentBase.OwnerNode);
            AvailableClicks["guild_leve_alchemist"] = (addon) => SendClick(addon, EventType.CHANGE, 15, ((AddonGuildLeve*)addon)->AlchemistButton->AtkComponentBase.OwnerNode);
            AvailableClicks["guild_leve_culinarian"] = (addon) => SendClick(addon, EventType.CHANGE, 16, ((AddonGuildLeve*)addon)->CulinarianButton->AtkComponentBase.OwnerNode);

            AvailableClicks["guild_leve_miner"] = (addon) => SendClick(addon, EventType.CHANGE, 9, ((AddonGuildLeve*)addon)->MinerButton->AtkComponentBase.OwnerNode);
            AvailableClicks["guild_leve_botanist"] = (addon) => SendClick(addon, EventType.CHANGE, 10, ((AddonGuildLeve*)addon)->BotanistButton->AtkComponentBase.OwnerNode);
            AvailableClicks["guild_leve_fisher"] = (addon) => SendClick(addon, EventType.CHANGE, 11, ((AddonGuildLeve*)addon)->FisherButton->AtkComponentBase.OwnerNode);
        }
    }
}
