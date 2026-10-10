// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/Nyanotrasen/NPC/Systems/FactionSystem.Items.cs
// (Misfits' copy of a Nyanotrasen file.) Changed for Wild Wanamingo: its own system instead of a NpcFactionSystem partial.

using Content.Shared.Clothing.Components;
using Content.Shared.Inventory.Events;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Systems;

namespace Content.Shared._Misfits.NPC.Legacy;

public sealed partial class ClothingAddFactionSystem : EntitySystem
{
    [Dependency] private NpcFactionSystem _faction = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ClothingAddFactionComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<ClothingAddFactionComponent, GotUnequippedEvent>(OnUnequipped);
    }

    private void OnEquipped(EntityUid uid, ClothingAddFactionComponent component, GotEquippedEvent args)
    {
        if (!TryComp<ClothingComponent>(uid, out var clothing) || !clothing.Slots.HasFlag(args.SlotFlags))
            return;

        if (!TryComp<NpcFactionMemberComponent>(args.EquipTarget, out var member)
            || _faction.IsMember((args.EquipTarget, member), component.Faction))
            return;

        component.IsActive = true;
        _faction.AddFaction((args.EquipTarget, member), component.Faction);
    }

    private void OnUnequipped(EntityUid uid, ClothingAddFactionComponent component, GotUnequippedEvent args)
    {
        if (!component.IsActive)
            return;

        component.IsActive = false;
        _faction.RemoveFaction(args.EquipTarget, component.Faction);
    }
}
