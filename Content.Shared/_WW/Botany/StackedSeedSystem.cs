using Content.Shared.Botany.Components;
using Content.Shared.Botany.Events;
using Content.Shared.Botany.Items.Components;
using Content.Shared.Botany.Items.Systems;
using Content.Shared.Botany.Systems;
using Content.Shared.Interaction;
using Content.Shared.Stacks;
using Robust.Shared.Network;

namespace Content.Shared._WW.Botany;

/// <summary>
/// Planting from a stack of plantable produce (wild agave leaves, zoybeans...) uses one item, not the whole stack.
/// Wizden's planting deletes the seed entity, which is fine for seed packets but not for stacks.
/// </summary>
public sealed partial class StackedSeedSystem : EntitySystem
{
    [Dependency] private INetManager _net = default!;
    [Dependency] private PlantTraySystem _plantTray = default!;
    [Dependency] private SharedStackSystem _stack = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Wizden already subscribes Seed + AfterInteract, so hook the stack side and run first.
        SubscribeLocalEvent<StackComponent, AfterInteractEvent>(OnAfterInteract, before: [typeof(BotanySeedSystem)]);
    }

    private void OnAfterInteract(Entity<StackComponent> ent, ref AfterInteractEvent args)
    {
        var stack = ent.Comp;
        if (args.Handled || !args.CanReach || args.Target is not { } tray || !HasComp<PlantTrayComponent>(tray) ||
            !TryComp<SeedComponent>(ent, out var seedComp) || stack.Count <= 1)
        {
            return;
        }

        args.Handled = true;

        // Splitting spawns an entity, so only the server does it; the client just doesn't predict the old path.
        if (!_net.IsServer || _plantTray.HasPlant(tray))
        {
            if (_net.IsServer)
                RaisePlantAttempt(tray, (ent, seedComp), args.User); // let botany show its "already seeded" popup
            return;
        }

        if (_stack.Split((ent, stack), 1, Transform(ent).Coordinates, args.User) is not { } single ||
            !TryComp<SeedComponent>(single, out var seed))
        {
            return;
        }

        RaisePlantAttempt(tray, (single, seed), args.User);
    }

    private void RaisePlantAttempt(EntityUid tray, Entity<SeedComponent> seed, EntityUid user)
    {
        var ev = new PlantingSeedAttemptEvent(seed, user);
        RaiseLocalEvent(tray, ref ev);
    }
}
