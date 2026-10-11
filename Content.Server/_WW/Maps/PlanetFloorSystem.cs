using Content.Shared.Maps;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;

namespace Content.Server._WW.Maps;

public sealed partial class PlanetFloorSystem : EntitySystem
{
    [Dependency] private ITileDefinitionManager _tileDefs = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedMapSystem _map = default!;

    private readonly List<(Vector2i, Tile)> _tiles = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<PlanetFloorComponent, TileChangedEvent>(OnTileChanged);
    }

    private void OnTileChanged(Entity<PlanetFloorComponent> ent, ref TileChangedEvent args)
    {
        var fallback = _tileDefs[ent.Comp.Fallback.Id].TileId;
        foreach (var change in args.Changes)
        {
            // The fallback itself can't be broken by players; it only goes away when the biome unloads a chunk.
            if (change.OldTile.TypeId == fallback || !IsSpace(change.NewTile))
                continue;

            ent.Comp.Pending.Add(change.GridIndices);
        }
    }

    private bool IsSpace(Tile tile)
    {
        if (tile.IsEmpty)
            return true;

        var def = (ContentTileDefinition) _tileDefs[tile.TypeId];
        return def.MapAtmosphere || def.ID == "Lattice";
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<PlanetFloorComponent, MapGridComponent>();
        while (query.MoveNext(out var uid, out var planet, out var grid))
        {
            if (planet.Pending.Count == 0)
                continue;

            var def = (ContentTileDefinition) _tileDefs[planet.Fallback.Id];
            _tiles.Clear();
            foreach (var indices in planet.Pending)
            {
                // Still space? (something may have built over it since)
                if (_map.TryGetTileRef(uid, grid, indices, out var tileRef) && !IsSpace(tileRef.Tile))
                    continue;

                var variant = def.PlacementVariants is { Length: > 0 } variants
                    ? (byte) _random.Next(variants.Length)
                    : (byte) 0;
                _tiles.Add((indices, new Tile(def.TileId, variant: variant)));
            }

            planet.Pending.Clear();
            if (_tiles.Count > 0)
                _map.SetTiles(uid, grid, _tiles);
        }
    }
}
