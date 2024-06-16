using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons.DalamudServices;
using System.Runtime.InteropServices;
using YesAlready.BaseFeatures;

namespace YesAlready.Features;

internal class AddonMKSRecord : BaseFeature
{
    private delegate void AbandonDuty(bool a1);
    private AbandonDuty _abandonDuty = null!;

    public override void Enable()
    {
        base.Enable();
        AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "MKSRecord", AddonSetup);
        _abandonDuty = Marshal.GetDelegateForFunctionPointer<AbandonDuty>(Svc.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 8B 43 28 B1 01"));
    }

    public override void Disable()
    {
        base.Disable();
        AddonLifecycle.UnregisterListener(AddonSetup);
    }

    protected unsafe void AddonSetup(AddonEvent eventType, AddonArgs addonInfo)
    {
        if (!P.Active || !P.Config.MKSRecordQuit)
            return;

        _abandonDuty(false);
    }
}
