// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_Misfits/Silicon/SiliconSelfCellEjectOnlySystem.cs
// Changed for Wild Wanamingo: popup text moved to locale.

using Content.Shared.Containers.ItemSlots;
using Content.Shared.Popups;

namespace Content.Shared._Misfits.Silicon;

public sealed partial class SiliconSelfCellEjectOnlySystem : EntitySystem
{
    [Dependency] private SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SiliconSelfCellEjectOnlyComponent, ItemSlotEjectAttemptEvent>(OnSlotEjectAttempt);
    }

    private void OnSlotEjectAttempt(Entity<SiliconSelfCellEjectOnlyComponent> ent, ref ItemSlotEjectAttemptEvent args)
    {
        if (args.Slot.ID != ent.Comp.SlotId || args.User == ent.Owner)
            return;

        args.Cancelled = true;
        if (args.User is { } user)
            _popup.PopupClient(Loc.GetString("silicon-self-cell-eject-only"), ent, user);
    }
}
