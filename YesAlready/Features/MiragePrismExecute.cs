namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostSetup)]
internal class MiragePrismExecute : AddonFeature
{
    protected override bool IsEnabled() => C.MiragePrismExecuteCast;

    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo, AtkUnitBase* atk) => new AddonMaster.MiragePrismExecute(atk).Cast();

    public partial class AddonMaster // TODO: wait for EC to merge
    {
        public unsafe class MiragePrismExecute : AddonMasterBase<AtkUnitBase>
        {
            public MiragePrismExecute(nint addon) : base(addon) { }
            public MiragePrismExecute(void* addon) : base(addon) { }
            public unsafe AtkComponentButton* CastButton => Addon->GetComponentButtonById(23);
            public void Cast() => ClickButtonIfEnabled(CastButton);
            public override string AddonDescription { get; } = "Cast glamour window";
        }
    }
}
