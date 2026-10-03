using System.Linq;
using Content.Server.Destructible;
using Content.Server.Destructible.Thresholds.Behaviors;
using Content.Shared.Damage.Systems;
using Content.Shared.Destructible;
using Content.Shared.Doors.Components;
using Content.Shared.Misfits.Administration.DoorLogs;
using Robust.Server.Player;
using Robust.Shared.Timing;

namespace Content.Server.Misfits.Administration.DoorLogs;

/// <summary>
/// Misfits: records which doors were destroyed or broken, and by whom, for the door logs admin panel.
/// Kept in memory for the current round only.
/// </summary>
public sealed partial class DoorLogSystem : EntitySystem
{
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IGameTiming _timing = default!;

    private readonly List<DoorLogEntry> _entries = new();
    private readonly HashSet<DoorLogsEui> _openUis = new();

    /// <summary>
    /// Last entity to damage each door, blamed when it breaks.
    /// </summary>
    private readonly Dictionary<EntityUid, EntityUid> _lastAttacker = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DoorComponent, DamageDealtEvent>(OnDoorDamaged);
        SubscribeLocalEvent<DoorComponent, DamageThresholdReached>(OnDoorThresholdReached);
        SubscribeLocalEvent<DoorComponent, ComponentShutdown>(OnDoorShutdown);
    }

    private void OnDoorDamaged(Entity<DoorComponent> ent, ref DamageDealtEvent args)
    {
        if (args.Origin is { } origin)
            _lastAttacker[ent] = origin;
    }

    private void OnDoorShutdown(Entity<DoorComponent> ent, ref ComponentShutdown args)
    {
        _lastAttacker.Remove(ent);
    }

    private void OnDoorThresholdReached(Entity<DoorComponent> ent, ref DamageThresholdReached args)
    {
        if (!args.Threshold.Behaviors.OfType<DoActsBehavior>()
                .Any(b => b.HasAct(ThresholdActs.Destruction) || b.HasAct(ThresholdActs.Breakage)))
        {
            return;
        }

        var destroyer = _lastAttacker.Remove(ent, out var attacker) ? AttackerName(attacker) : "Unknown";
        _entries.Add(new DoorLogEntry(MetaData(ent).EntityPrototype?.ID ?? "Unknown", destroyer, _timing.CurTime));

        foreach (var ui in _openUis)
        {
            if (!ui.IsShutDown)
                ui.StateDirty();
        }
    }

    private string AttackerName(EntityUid attacker)
    {
        return _player.TryGetSessionByEntity(attacker, out var session)
            ? session.Name
            : ToPrettyString(attacker).ToString();
    }

    public List<DoorLogEntry> GetEntries()
    {
        return _entries.OrderByDescending(e => e.Time).ToList();
    }

    public void RegisterUi(DoorLogsEui ui) => _openUis.Add(ui);

    public void UnregisterUi(DoorLogsEui ui) => _openUis.Remove(ui);
}
