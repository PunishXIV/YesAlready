namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
[AddonFeature(AddonEvent.PostUpdate, "SalvageAutoDialog")]
[Bother(nameof(Configuration.DesynthesisResults), BotherCategory.Desynthesis, "Automatically closes the SalvageResults window when done desynthesizing.")]
internal class SalvageResult : AddonFeature
{
    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo)
    {
        switch (addonInfo.AddonName)
        {
            case "SalvageResult":
                if (addonInfo.GetAddon<AddonSalvageResult>()->GetComponentButtonById(15) is not null and var closeBtn)
                    closeBtn->Click();
                break;

            case "SalvageAutoDialog":
                var addon = addonInfo.GetAddon<AddonSalvageAutoDialog>();
                if (!addon->IsDesynthesizing && addon->EndDesynthesisButton is not null)
                    addon->EndDesynthesisButton->Click();
                break;
        }
    }
}
