// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_Misfits/NPC/ProximityNPCComponent.cs

namespace Content.Shared._Misfits.NPC;

/// <summary>
/// Marks an NPC as using proximity-based sleep/wake.
/// The NPC starts asleep on map initialisation and wakes only when a player-controlled
/// entity enters <see cref="WakeRange"/> tiles. It re-sleeps when all players leave
/// <see cref="SleepRange"/> tiles.
///
/// Designed for large open maps where running full HTN AI on every creature continuously is too expensive.
/// </summary>
[RegisterComponent]
public sealed partial class ProximityNPCComponent : Component
{
    /// <summary>
    /// Distance (tiles) within which a player wakes this NPC.
    /// </summary>
    [DataField]
    public float WakeRange = 30f;

    /// <summary>
    /// Distance (tiles) at which the NPC sleeps if no players remain nearby.
    /// Greater than <see cref="WakeRange"/> so NPCs at the edge don't flip between awake and asleep.
    /// </summary>
    [DataField]
    public float SleepRange = 45f;

    /// <summary>
    /// If true, the NPC starts asleep instead of HTN's default of waking on map init.
    /// </summary>
    [DataField]
    public bool StartAsleep = true;
}
