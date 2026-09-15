using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace YesAlready.Features;

[AddonFeature(AddonEvent.PostRefresh)]
[Bother(nameof(Configuration.DesynthDialogEnabled), BotherCategory.Desynthesis, "Remove the Desynthesis menu confirmation.")]
[Bother(nameof(Configuration.DesynthBulkDialogEnabled), BotherCategory.Desynthesis, "Check \"Desynthesize entire stack\" when confirming desynthesis.",
    label: "Entire stack", RequiresEnabledProperty = nameof(Configuration.DesynthDialogEnabled), ContributesToEnable = false)]
[Bother(nameof(Configuration.DesynthGuaranteeNQEnabled), BotherCategory.Desynthesis, "Check \"Guarantee NQ item results\" when confirming desynthesis.",
    label: "Guarantee NQ", RequiresEnabledProperty = nameof(Configuration.DesynthDialogEnabled), ContributesToEnable = false)]
internal class SalvageDialog : AddonFeature
{
    protected override unsafe void HandleAddonEvent(AddonEvent eventType, AddonArgs addonInfo)
    {
        var addon = (LAddonSalvageDialog*)addonInfo.GetAddon<AddonSalvageDialog>();
        if (C.DesynthBulkDialogEnabled && addon->TryGetCheckbox(SalvageDialogCheckboxType.EntireStack, out var stack) && !stack->IsChecked)
            stack->Click();
        if (C.DesynthGuaranteeNQEnabled && addon->TryGetCheckbox(SalvageDialogCheckboxType.GuaranteeNQ, out var nq) && !nq->IsChecked)
            nq->Click();
        if (C.DesynthDialogEnabled)
            addon->DesynthesizeButton->Click();
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x2A0)]
    public unsafe struct LAddonSalvageDialog
    {
        [FieldOffset(0x248)] public CheckboxSlot Checkbox0;
        [FieldOffset(0x260)] public CheckboxSlot Checkbox1;
        [FieldOffset(0x278)] public CheckboxSlot Checkbox2;

        [FieldOffset(0x290)] public AtkComponentButton* DesynthesizeButton;
        [FieldOffset(0x298)] public AtkComponentButton* CancelButton;

        public CheckboxSlot* Checkboxes => (CheckboxSlot*)((byte*)Unsafe.AsPointer(ref this) + 0x248);

        public ref CheckboxSlot this[int index] => ref Checkboxes[index];

        public bool TryGetCheckbox(SalvageDialogCheckboxType type, out AtkComponentCheckBox* box)
        {
            for (var i = 0; i < 3; i++)
            {
                ref var slot = ref Checkboxes[i];
                if (slot.Type == type && slot.Checkbox != null)
                {
                    box = slot.Checkbox;
                    return box->OwnerNode->AtkResNode.IsActuallyVisible;
                }
            }
            box = null;
            return false;
        }

        [StructLayout(LayoutKind.Explicit, Size = 0x18)]
        public struct CheckboxSlot
        {
            [FieldOffset(0x00)] public SalvageDialogCheckboxType Type;
            [FieldOffset(0x08)] public AtkComponentCheckBox* Checkbox;
            [FieldOffset(0x10)] public bool IsCheckedCached;
        }
    }

    public enum SalvageDialogCheckboxType : byte
    {
        EntireStack = 0,
        Warning = 1, // unique / materia / collectable w/e warning
        GuaranteeNQ = 2,
    }
}
