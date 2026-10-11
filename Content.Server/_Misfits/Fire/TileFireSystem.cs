// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Server/_Misfits/Fire/TileFireSystem.cs
// Changed for Wild Wanamingo: Wizden damage/flammable APIs; standing damage goes through fire protection only.

using Content.Server.Atmos.EntitySystems;
using Content.Server.Chemistry.Components;
using Content.Shared._Misfits.Fire;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.Inventory;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._Misfits.Fire;

public sealed partial class TileFireSystem : EntitySystem
{
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private FlammableSystem _flammable = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private SharedSolutionContainerSystem _solution = default!;

    private static readonly ProtoId<ReagentPrototype> WaterReagent = "Water";
    private static readonly ProtoId<DamageTypePrototype> HeatDamage = "Heat";

    private EntityQuery<FlammableComponent> _flammableQuery;
    private EntityQuery<SolutionManagerComponent> _solutionManagerQuery;
    private EntityQuery<VaporComponent> _vaporQuery;

    private readonly HashSet<EntityUid> _touching = new();

    public override void Initialize()
    {
        _flammableQuery = GetEntityQuery<FlammableComponent>();
        _solutionManagerQuery = GetEntityQuery<SolutionManagerComponent>();
        _vaporQuery = GetEntityQuery<VaporComponent>();

        SubscribeLocalEvent<TileFireComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<TileFireComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.SpawnedAt = _timing.CurTime;
        Dirty(ent);
        UpdateVisualStage(ent);
    }

    private void TryExtinguishFromVapor(Entity<TileFireComponent> fire, EntityUid vapor)
    {
        if (!_solutionManagerQuery.TryComp(vapor, out var manager))
            return;

        foreach (var (_, solution) in _solution.EnumerateSolutions((vapor, manager)))
        {
            if (!solution.Comp.Solution.ContainsPrototype(WaterReagent))
                continue;

            if (fire.Comp.ExtinguishInstantly)
            {
                QueueDel(fire);
                return;
            }

            fire.Comp.Duration -= TimeSpan.FromSeconds(fire.Comp.VaporExtinguishSeconds);
            Dirty(fire);
            UpdateVisualStage(fire);
            return;
        }
    }

    private void UpdateVisualStage(Entity<TileFireComponent> fire)
    {
        var elapsed = _timing.CurTime - fire.Comp.SpawnedAt;
        var remaining = fire.Comp.Duration - elapsed;

        if (remaining <= TimeSpan.Zero)
            return;

        var visual = TileFireVisuals.Three;
        if (elapsed < fire.Comp.BigFireDuration)
            visual = TileFireVisuals.Four;
        else if (remaining <= fire.Comp.Duration * 0.33f)
            visual = TileFireVisuals.One;
        else if (remaining <= fire.Comp.Duration * 0.66f)
            visual = TileFireVisuals.Two;

        _appearance.SetData(fire, TileFireLayers.Base, visual);
    }

    private void ProcessTouch(Entity<TileFireComponent> fire, EntityUid target)
    {
        var touch = EnsureComp<TileFireTouchComponent>(target);
        if (_timing.CurTime < touch.NextTouchAt)
            return;

        touch.NextTouchAt = _timing.CurTime + fire.Comp.TouchCooldown;

        // Wizden's FlammableSystem assumes an Appearance (some items, e.g. envelopes, lack one and log errors).
        if (_flammableQuery.TryComp(target, out var flammable) && HasComp<AppearanceComponent>(target))
        {
            _flammable.AdjustFireStacks(target, fire.Comp.FireStacks, flammable);
            _flammable.Ignite(target, fire.Owner, flammable);
        }

        if (fire.Comp.StandingDamage <= 0)
            return;

        var ev = new GetFireProtectionEvent();
        RaiseLocalEvent(target, ref ev);
        if (TryComp<InventoryComponent>(target, out var inventory))
            _inventory.RelayEvent((target, inventory), ref ev);

        var damage = fire.Comp.StandingDamage * ev.Multiplier;
        if (damage <= 0)
            return;

        _damageable.TryChangeDamage(target, new DamageSpecifier { DamageDict = { [HeatDamage] = damage } }, interruptsDoAfters: false);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<TileFireComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var fire, out var xform))
        {
            var elapsed = _timing.CurTime - fire.SpawnedAt;
            if (elapsed >= fire.Duration)
            {
                QueueDel(uid);
                continue;
            }

            UpdateVisualStage((uid, fire));

            _touching.Clear();
            _lookup.GetEntitiesInRange(xform.Coordinates, 0.45f, _touching, LookupFlags.Dynamic);
            foreach (var entity in _touching)
            {
                if (entity == uid)
                    continue;

                if (_vaporQuery.HasComp(entity))
                {
                    TryExtinguishFromVapor((uid, fire), entity);
                    continue;
                }

                ProcessTouch((uid, fire), entity);
            }
        }
    }
}
