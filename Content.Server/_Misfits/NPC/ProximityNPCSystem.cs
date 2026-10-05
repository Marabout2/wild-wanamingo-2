// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Server/_Misfits/NPC/ProximityNPCSystem.cs
// Changed for Wild Wanamingo: the check interval is a constant instead of a Misfits CVar, and the recruited
// follower exception was dropped (followers aren't ported).

using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Shared._Misfits.NPC;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Components;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._Misfits.NPC;

/// <summary>
/// Keeps NPCs with <see cref="ProximityNPCComponent"/> asleep until a player enters
/// their wake radius, then re-sleeps them when all players leave.
/// </summary>
/// <remarks>
/// Uses a work queue: every <see cref="CheckInterval"/> seconds it snapshots all proximity NPCs, then
/// processes a small batch each tick so the spatial queries are spread evenly across ticks.
/// </remarks>
public sealed partial class ProximityNPCSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private NPCSystem _npc = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    /// <summary>
    /// Seconds between full passes over all proximity NPCs.
    /// </summary>
    private const float CheckInterval = 5f;

    private float _accumulator;

    // Work queue: snapshot of NPCs to check, processed across multiple ticks.
    private readonly List<EntityUid> _pending = new();
    private int _pendingIndex;
    private int _budgetPerTick;

    // Reused across calls to avoid allocating a new HashSet per NPC per scan.
    private readonly HashSet<Entity<ActorComponent>> _playerBuffer = new();

    private EntityQuery<TransformComponent> _xformQuery;

    public override void Initialize()
    {
        base.Initialize();

        // After HTNSystem so our sleep call overrides HTN's default WakeNPC on map init.
        SubscribeLocalEvent<ProximityNPCComponent, MapInitEvent>(OnMapInit, after: [typeof(HTNSystem)]);

        // If an admin possesses a sleeping NPC, make sure it can accept input.
        SubscribeLocalEvent<ProximityNPCComponent, PlayerAttachedEvent>(OnPlayerAttached);
        _xformQuery = GetEntityQuery<TransformComponent>();
    }

    private void OnMapInit(Entity<ProximityNPCComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.StartAsleep)
            _npc.SleepNPC(ent);
    }

    private void OnPlayerAttached(Entity<ProximityNPCComponent> ent, ref PlayerAttachedEvent args)
    {
        EnsureComp<InputMoverComponent>(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Keep working through the current snapshot before taking a new one.
        if (_pendingIndex < _pending.Count)
        {
            ProcessBatch();
            return;
        }

        _accumulator += frameTime;
        if (_accumulator < CheckInterval)
            return;
        _accumulator -= CheckInterval;

        _pending.Clear();
        var query = EntityQueryEnumerator<ProximityNPCComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            _pending.Add(uid);
        }

        if (_pending.Count == 0)
            return;

        _pendingIndex = 0;

        // Spread evenly so every NPC is checked once per interval.
        var ticksAvailable = CheckInterval * _timing.TickRate;
        _budgetPerTick = Math.Max(1, (int) Math.Ceiling(_pending.Count / ticksAvailable));

        ProcessBatch();
    }

    /// <summary>
    /// Processes up to <see cref="_budgetPerTick"/> NPCs from the pending queue.
    /// </summary>
    private void ProcessBatch()
    {
        var end = Math.Min(_pendingIndex + _budgetPerTick, _pending.Count);

        for (var i = _pendingIndex; i < end; i++)
        {
            var uid = _pending[i];
            if (!TryComp(uid, out ProximityNPCComponent? prox) ||
                !_xformQuery.TryGetComponent(uid, out var xform) ||
                xform.MapID == MapId.Nullspace ||
                !TryComp(uid, out MobStateComponent? state) ||
                !TryComp(uid, out HTNComponent? htn))
            {
                continue;
            }

            // Player-possessed mobs are left alone: HTN already sleeps their AI while controlled.
            if (HasComp<ActorComponent>(uid))
                continue;

            var mapPos = _transform.GetMapCoordinates(uid, xform);
            var awake = _npc.IsAwake(uid, htn);

            if (awake && !HasPlayerWithin(mapPos, prox.SleepRange))
                _npc.SleepNPC(uid, htn);
            // Crit or dead NPCs stay asleep so they don't move.
            else if (!awake && state.CurrentState == MobState.Alive && HasPlayerWithin(mapPos, prox.WakeRange))
                _npc.WakeNPC(uid, htn);
        }

        _pendingIndex = end;
    }

    /// <summary>
    /// True if a player-controlled entity is within <paramref name="range"/> tiles of <paramref name="pos"/>.
    /// </summary>
    private bool HasPlayerWithin(MapCoordinates pos, float range)
    {
        _playerBuffer.Clear();
        _lookup.GetEntitiesInRange(pos, range, _playerBuffer);
        return _playerBuffer.Count > 0;
    }
}
