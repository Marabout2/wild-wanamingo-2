using Content.Server.Administration.Logs;
using Content.Server.Administration.Managers;
using Content.Shared.Database;
using Content.Shared.Ghost.Components;
using Content.Shared.Misfits.Administration;

namespace Content.Server.Misfits.Administration.Systems;

/// <summary>
/// Misfits: lets an admin in aghost drag any entity to empty space to teleport it there.
/// Admin and aghost state are checked here, never trusted from the client.
/// </summary>
public sealed partial class AdminDragTeleportSystem : EntitySystem
{
    [Dependency] private IAdminManager _admin = default!;
    [Dependency] private IAdminLogManager _adminLog = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<AdminSelfDragTeleportEvent>(OnDragTeleport);
    }

    private void OnDragTeleport(AdminSelfDragTeleportEvent ev, EntitySessionEventArgs args)
    {
        if (!_admin.IsAdmin(args.SenderSession))
            return;

        if (args.SenderSession.AttachedEntity is not { } sender
            || !TryComp<GhostComponent>(sender, out var ghost)
            || !ghost.CanGhostInteract)
        {
            return;
        }

        var entity = GetEntity(ev.DraggedEntity);
        if (!Exists(entity))
            return;

        var coords = GetCoordinates(ev.TargetCoordinates);
        if (!coords.IsValid(EntityManager))
            return;

        _transform.SetCoordinates(entity, coords);
        _transform.AttachToGridOrMap(entity);

        _adminLog.Add(LogType.Action, LogImpact.Low,
            $"{ToPrettyString(sender):actor} drag-teleported {ToPrettyString(entity):subject} to {coords}");
    }
}
