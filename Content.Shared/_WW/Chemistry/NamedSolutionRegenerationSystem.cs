using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;
using Robust.Shared.Timing;

namespace Content.Shared._WW.Chemistry;

public sealed partial class NamedSolutionRegenerationSystem : EntitySystem
{
    [Dependency] private SharedSolutionContainerSystem _solution = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NamedSolutionRegenerationComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<NamedSolutionRegenerationComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextRegenTime = _timing.CurTime + ent.Comp.Duration;
        Dirty(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<NamedSolutionRegenerationComponent>();
        while (query.MoveNext(out var uid, out var regen))
        {
            if (_timing.CurTime < regen.NextRegenTime)
                continue;

            // Fixed cycle, whether or not the solution is full.
            regen.NextRegenTime += regen.Duration;
            Dirty(uid, regen);

            if (!_solution.TryGetSolution(uid, regen.Solution, out var solutionEnt, out var solution))
                continue;

            var amount = FixedPoint2.Min(solution.AvailableVolume, regen.Generated.Volume);
            if (amount <= FixedPoint2.Zero)
                continue;

            var generated = amount == regen.Generated.Volume
                ? regen.Generated
                : regen.Generated.Clone().SplitSolution(amount);

            _solution.TryAddSolution(solutionEnt.Value, generated);
        }
    }
}
