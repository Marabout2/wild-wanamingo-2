// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_Misfits/Storage/PauseTimedDespawnInEntityStorageComponent.cs

using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared._Misfits.Storage;

/// <summary>
/// Marks entities whose timed despawn should pause while they are stored in entity storage.
/// </summary>
[RegisterComponent]
public sealed partial class PauseTimedDespawnInEntityStorageComponent : Component
{
    [DataField]
    public float? PausedLifetime;
}