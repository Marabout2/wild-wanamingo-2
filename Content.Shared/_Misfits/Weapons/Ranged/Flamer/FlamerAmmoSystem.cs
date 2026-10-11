// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_Misfits/Weapons/Ranged/Flamer/SharedFlamerAmmoSystem.cs
// Changed for Wild Wanamingo: shots are hitscan entities spawned on the server; the client only predicts the sound.

using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Weapons.Hitscan.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Network;

namespace Content.Shared._Misfits.Weapons.Ranged.Flamer;

public sealed partial class FlamerAmmoSystem : EntitySystem
{
    [Dependency] private INetManager _net = default!;
    [Dependency] private SharedSolutionContainerSystem _solution = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<FlamerAmmoProviderComponent, TakeAmmoEvent>(OnTakeAmmo);
        SubscribeLocalEvent<FlamerAmmoProviderComponent, GetAmmoCountEvent>(OnGetAmmoCount);
        SubscribeLocalEvent<FlamerAmmoProviderComponent, AttemptShootEvent>(OnAttemptShoot);
    }

    private void OnTakeAmmo(Entity<FlamerAmmoProviderComponent> ent, ref TakeAmmoEvent args)
    {
        if (!_solution.TryGetSolution(ent.Owner, ent.Comp.SolutionId, out var solutionEnt, out var solution))
            return;

        if (solution.Volume < ent.Comp.FireCost * args.Shots)
        {
            args.Reason = Loc.GetString("gun-no-magazine");
            return;
        }

        for (var i = 0; i < args.Shots; i++)
        {
            _solution.SplitSolution(solutionEnt.Value, ent.Comp.FireCost);

            if (_net.IsClient)
            {
                args.Ammo.Add((null, new HitscanAmmoComponent()));
                continue;
            }

            var shot = SpawnAtPosition(ent.Comp.ShotPrototype, args.Coordinates);
            var flamerShot = EnsureComp<FlamerShotComponent>(shot);
            flamerShot.FireTilePrototype = ent.Comp.FireTilePrototype;
            flamerShot.ProjectileProto = ent.Comp.ProjectileProto;
            flamerShot.Range = ent.Comp.Range;
            args.Ammo.Add((shot, EnsureComp<HitscanAmmoComponent>(shot)));
        }
    }

    private void OnGetAmmoCount(Entity<FlamerAmmoProviderComponent> ent, ref GetAmmoCountEvent args)
    {
        if (!_solution.TryGetSolution(ent.Owner, ent.Comp.SolutionId, out _, out var solution))
            return;

        args.Count = (int) (solution.Volume / ent.Comp.FireCost);
        args.Capacity = (int) (solution.MaxVolume / ent.Comp.FireCost);
    }

    private void OnAttemptShoot(Entity<FlamerAmmoProviderComponent> ent, ref AttemptShootEvent args)
    {
        if (!_solution.TryGetSolution(ent.Owner, ent.Comp.SolutionId, out _, out var solution) ||
            solution.Volume < ent.Comp.FireCost)
        {
            args.Cancelled = true;
            args.Message = Loc.GetString("gun-no-magazine");
        }
    }
}
