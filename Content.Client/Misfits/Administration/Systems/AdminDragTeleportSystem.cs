using Content.Client.Administration.Managers;
using Content.Client.Misfits.Administration.Events;
using Content.Shared.DragDrop;
using Content.Shared.Ghost.Components;
using Content.Shared.Misfits.Administration;
using Robust.Client.GameObjects;
using Robust.Client.Player;

namespace Content.Client.Misfits.Administration.Systems;

/// <summary>
/// Misfits: while in aghost, admins can drag any visible entity and drop it on empty space to teleport it there.
/// The server re-checks permissions.
/// </summary>
public sealed partial class AdminDragTeleportSystem : EntitySystem
{
    [Dependency] private IClientAdminManager _admin = default!;
    [Dependency] private IPlayerManager _player = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpriteComponent, CanDragEvent>(OnCanDrag);
        SubscribeLocalEvent<DragNoTargetEvent>(OnDragNoTarget);
    }

    private bool IsAdminGhost()
    {
        return _admin.IsAdmin()
            && _player.LocalEntity is { } local
            && TryComp<GhostComponent>(local, out var ghost)
            && ghost.CanGhostInteract;
    }

    private void OnCanDrag(Entity<SpriteComponent> ent, ref CanDragEvent args)
    {
        if (IsAdminGhost())
            args.Handled = true;
    }

    private void OnDragNoTarget(DragNoTargetEvent ev)
    {
        if (!IsAdminGhost())
            return;

        RaiseNetworkEvent(new AdminSelfDragTeleportEvent(GetNetEntity(ev.DraggedEntity), GetNetCoordinates(ev.TargetCoordinates)));
        ev.Handled = true;
    }
}
