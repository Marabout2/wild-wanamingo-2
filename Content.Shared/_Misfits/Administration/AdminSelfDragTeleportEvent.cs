// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_Misfits/Administration/AdminSelfDragTeleportEvent.cs

using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._Misfits.Administration;

/// <summary>
/// Misfits: sent by an admin ghost dragging an entity to empty space, asking the server to teleport it there.
/// The server re-checks admin and aghost state before acting.
/// </summary>
[Serializable, NetSerializable]
public sealed class AdminSelfDragTeleportEvent : EntityEventArgs
{
    public readonly NetEntity DraggedEntity;
    public readonly NetCoordinates TargetCoordinates;

    public AdminSelfDragTeleportEvent(NetEntity draggedEntity, NetCoordinates targetCoordinates)
    {
        DraggedEntity = draggedEntity;
        TargetCoordinates = targetCoordinates;
    }
}
