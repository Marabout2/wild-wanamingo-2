// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Client/_Misfits/Administration/Events/DragNoTargetEvent.cs

using Robust.Shared.Map;

namespace Content.Client._Misfits.Administration.Events;

/// <summary>
/// Misfits: raised on the client when a drag-drop ends without a valid drop target, so admin
/// drag-teleport can use the release position. Set <see cref="Handled"/> to count it as a successful drop.
/// </summary>
public sealed class DragNoTargetEvent : EntityEventArgs
{
    public readonly EntityUid DraggedEntity;
    public readonly EntityCoordinates TargetCoordinates;
    public bool Handled;

    public DragNoTargetEvent(EntityUid draggedEntity, EntityCoordinates targetCoordinates)
    {
        DraggedEntity = draggedEntity;
        TargetCoordinates = targetCoordinates;
    }
}
