// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Server/_NC/SpawnWhenOpened/SpawnWhenOpenedSystem.cs
// (Misfits' copy of a Corvax Forge file.)
// Changed for Wild Wanamingo: Wizden analyzer conventions; state is dirtied when opened.

using Content.Shared._Misfits.Nutrition.Legacy;
using Content.Shared.Destructible;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Storage;
using Robust.Shared.Random;

namespace Content.Server._Misfits.Nutrition.Legacy;

public sealed partial class SpawnWhenOpenedSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SpawnWhenOpenedComponent, OpenableOpenedEvent>(OnOpened);
        SubscribeLocalEvent<SpawnWhenOpenedComponent, DestructionEventArgs>(OnDestroyed);
    }

    private void OnOpened(Entity<SpawnWhenOpenedComponent> ent, ref OpenableOpenedEvent args)
    {
        if (!ent.Comp.IsRepeatable && ent.Comp.IsAlreadyOpened)
            return;

        ent.Comp.IsAlreadyOpened = true;
        Dirty(ent);
        foreach (var proto in EntitySpawnCollection.GetSpawns(ent.Comp.Prototypes, _random))
        {
            var item = SpawnNextToOrDrop(proto, ent);
            if (args.User is { } user)
                _hands.TryPickupAnyHand(user, item);
        }
    }

    private void OnDestroyed(EntityUid uid, SpawnWhenOpenedComponent comp, DestructionEventArgs args)
    {
        if (!comp.IsRepeatable && comp.IsAlreadyOpened)
            return;

        comp.IsAlreadyOpened = true;
        foreach (var proto in EntitySpawnCollection.GetSpawns(comp.Prototypes, _random))
        {
            SpawnNextToOrDrop(proto, uid);
        }
    }
}
