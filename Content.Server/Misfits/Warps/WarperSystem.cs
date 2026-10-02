using System.Numerics;
using Content.Shared.Examine;
using Content.Shared.Ghost.Components;
using Content.Shared.Interaction;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Popups;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

namespace Content.Server.Misfits.Warps;

/// <summary>
/// Misfits: moves users between linked <see cref="WarperComponent"/>s (ladders, manholes, bunker hatches).
/// Ported from Nuclear 14's warper, but links ladder to ladder instead of ladder to a separate warp point.
/// </summary>
public sealed partial class WarperSystem : EntitySystem
{
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private PullingSystem _pulling = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WarperComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<WarperComponent, ActivateInWorldEvent>(OnActivateInWorld);
        SubscribeLocalEvent<WarperComponent, ExaminedEvent>(OnExamined);
    }

    private void OnInteractHand(Entity<WarperComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = TryWarp(ent, args.User);
    }

    private void OnActivateInWorld(Entity<WarperComponent> ent, ref ActivateInWorldEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = TryWarp(ent, args.User);
    }

    private void OnExamined(Entity<WarperComponent> ent, ref ExaminedEvent args)
    {
        // Ghosts that can't interact use close-range examine to travel through ladders.
        if (!args.IsInDetailsRange || !TryComp<GhostComponent>(args.Examiner, out var ghost) || ghost.CanGhostInteract)
            return;

        TryWarp(ent, args.Examiner);
    }

    private bool TryWarp(Entity<WarperComponent> ent, EntityUid user)
    {
        if (FindDestination(ent) is not { } dest || !WarpEntityTo(user, dest))
        {
            _popup.PopupEntity(Loc.GetString("warper-goes-nowhere", ("warper", ent.Owner)), user, user);
            return false;
        }

        return true;
    }

    /// <summary>
    /// The other warper with the same id.
    /// </summary>
    public EntityUid? FindDestination(Entity<WarperComponent> ent)
    {
        if (string.IsNullOrEmpty(ent.Comp.ID))
            return null;

        var query = EntityQueryEnumerator<WarperComponent>();
        while (query.MoveNext(out var uid, out var other))
        {
            if (uid != ent.Owner && other.ID == ent.Comp.ID)
                return uid;
        }

        return null;
    }

    /// <summary>
    /// Moves the user onto the destination, bringing whatever they are pulling.
    /// </summary>
    public bool WarpEntityTo(EntityUid user, EntityUid destination)
    {
        var destXform = Transform(destination);

        // Destination maps that aren't running are only reachable by admin ghosts.
        var destMap = destXform.MapID;
        if ((!_map.MapExists(destMap) || !_map.IsInitialized(destMap) || _map.IsPaused(destMap))
            && !HasComp<GhostComponent>(user))
        {
            return false;
        }

        EntityUid? pulled = null;
        if (TryComp<PullerComponent>(user, out var puller) && puller.Pulling is { } pulling)
        {
            pulled = pulling;
            _transform.SetCoordinates(pulling, destXform.Coordinates);
            _transform.AttachToGridOrMap(pulling);
        }

        _transform.SetCoordinates(user, destXform.Coordinates);
        _transform.AttachToGridOrMap(user);

        if (pulled != null)
            _pulling.TryStartPull(user, pulled.Value);

        if (HasComp<PhysicsComponent>(user))
            _physics.SetLinearVelocity(user, Vector2.Zero);

        return true;
    }
}
