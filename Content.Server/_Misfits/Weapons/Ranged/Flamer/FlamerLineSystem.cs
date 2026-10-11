// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Server/_Misfits/Weapons/Ranged/Flamer/FlamerLineSystem.cs
// Changed for Wild Wanamingo: fires from Wizden's HitscanTraceEvent instead of a GunSystem case; the line ends at Range along the shot direction.

using Content.Shared._Misfits.Fire;
using Content.Shared._Misfits.Weapons.Ranged.Flamer;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Weapons.Hitscan.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server._Misfits.Weapons.Ranged.Flamer;

public sealed partial class FlamerLineSystem : EntitySystem
{
    [Dependency] private SharedMapSystem _mapSystem = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private SharedGunSystem _gun = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private TurfSystem _turf = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<FlamerShotComponent, HitscanTraceEvent>(OnTrace);
    }

    private void OnTrace(Entity<FlamerShotComponent> ent, ref HitscanTraceEvent args)
    {
        var direction = args.ShotDirection;
        if (ent.Comp.ProjectileProto is { } projectileProto)
        {
            var projectile = Spawn(projectileProto, args.FromCoordinates);
            _gun.ShootProjectile(projectile, direction, _physics.GetMapLinearVelocity(args.Gun), args.Gun, args.Shooter);
        }

        var from = _transform.ToMapCoordinates(args.FromCoordinates);
        var target = from.Offset(direction * ent.Comp.Range);
        ShootLine(ent.Comp.FireTilePrototype, ent.Comp.Range, from, target);
    }

    public void ShootLine(EntProtoId fireTilePrototype, int range, MapCoordinates fromMap, MapCoordinates targetMap)
    {
        if (fromMap.MapId != targetMap.MapId ||
            !_mapSystem.TryFindGridAt(fromMap, out var gridUid, out var grid))
        {
            return;
        }

        var start = _mapSystem.TileIndicesFor(gridUid, grid, fromMap);
        var target = _mapSystem.TileIndicesFor(gridUid, grid, targetMap);

        var tiles = GetLine(start, target, range);

        // The first tile is the shooter's own.
        for (var i = 1; i < tiles.Count; i++)
        {
            if (!_mapSystem.TryGetTileRef(gridUid, grid, tiles[i], out var tileRef) || tileRef.Tile.IsEmpty)
                break;

            if (_turf.IsTileBlocked(tileRef, CollisionGroup.Impassable))
                break;

            SpawnOrRefreshTileFire(fireTilePrototype, _mapSystem.GridTileToLocal(gridUid, grid, tiles[i]));
        }
    }

    public void SpawnDiamond(EntProtoId fireTilePrototype, EntityCoordinates center, int range)
    {
        if (_transform.GetGrid(center) is not { } gridUid ||
            !TryComp<MapGridComponent>(gridUid, out var grid))
        {
            return;
        }

        var origin = _mapSystem.TileIndicesFor(gridUid, grid, center);
        for (var x = -range; x <= range; x++)
        {
            for (var y = -range; y <= range; y++)
            {
                if (Math.Abs(x) + Math.Abs(y) > range)
                    continue;

                var tile = origin + new Vector2i(x, y);
                if (!_mapSystem.TryGetTileRef(gridUid, grid, tile, out var tileRef) || tileRef.Tile.IsEmpty)
                    continue;

                SpawnOrRefreshTileFire(fireTilePrototype, _mapSystem.GridTileToLocal(gridUid, grid, tile));
            }
        }
    }

    private void SpawnOrRefreshTileFire(EntProtoId fireTilePrototype, EntityCoordinates coords)
    {
        foreach (var uid in _lookup.GetEntitiesInRange<TileFireComponent>(coords, 0.4f, LookupFlags.Static | LookupFlags.Dynamic))
        {
            QueueDel(uid);
        }

        Spawn(fireTilePrototype, coords);
    }

    private static List<Vector2i> GetLine(Vector2i start, Vector2i end, int maxSteps)
    {
        var points = new List<Vector2i>();

        var x0 = start.X;
        var y0 = start.Y;
        var x1 = end.X;
        var y1 = end.Y;

        var dx = Math.Abs(x1 - x0);
        var sx = x0 < x1 ? 1 : -1;
        var dy = -Math.Abs(y1 - y0);
        var sy = y0 < y1 ? 1 : -1;
        var error = dx + dy;
        var steps = 0;

        while (true)
        {
            points.Add(new Vector2i(x0, y0));
            if ((x0 == x1 && y0 == y1) || steps >= maxSteps)
                break;

            var e2 = 2 * error;
            if (e2 >= dy)
            {
                error += dy;
                x0 += sx;
            }

            if (e2 <= dx)
            {
                error += dx;
                y0 += sy;
            }

            steps++;
        }

        return points;
    }
}
