using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._WW.Spawners;

/// <summary>
/// Periodically spawns mobs or items, but only while someone is around to meet them: it stays dormant with no
/// living player in <see cref="ActivationRange"/>, never spawns in view of one (<see cref="SpawnBlockRange"/>), and
/// stops once <see cref="MaxAlivePerPrototype"/> of its mobs are alive on the map. Used by the N14/Misfits timed
/// spawners (molerats, pigrats...), so wasteland creatures don't pile up over a long round.
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class WastelandSpawnerComponent : Component
{
    [DataField(required: true)]
    public List<EntProtoId> Prototypes = new();

    /// <summary>
    /// Chance that a due spawn actually happens.
    /// </summary>
    [DataField]
    public float Chance = 1f;

    [DataField]
    public int IntervalSeconds = 60;

    [DataField]
    public int MinimumEntitiesSpawned = 1;

    [DataField]
    public int MaximumEntitiesSpawned = 1;

    /// <summary>
    /// Only spawn when a living player is within this many tiles. 0 = always active.
    /// </summary>
    [DataField]
    public float ActivationRange;

    /// <summary>
    /// Don't spawn while a living player is this close (no mobs popping in on screen).
    /// </summary>
    [DataField]
    public float SpawnBlockRange = 15f;

    /// <summary>
    /// Spawn even with players nearby (e.g. animal droppings).
    /// </summary>
    [DataField]
    public bool IgnoreSpawnBlock;

    /// <summary>
    /// Stop while this many living entities of this spawner's prototypes are on the map. 0 = unlimited.
    /// </summary>
    [DataField]
    public int MaxAlivePerPrototype;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextFire;
}
