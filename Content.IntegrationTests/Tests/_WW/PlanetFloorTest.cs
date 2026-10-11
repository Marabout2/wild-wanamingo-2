#nullable enable
using Content.IntegrationTests.Fixtures;
using Content.Server._WW.Maps;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Maths;

namespace Content.IntegrationTests.Tests._WW;

/// <summary>
/// On a planet, floors that get removed turn into sand instead of space. Sand itself may be removed (the biome
/// unloading chunks), and is left alone.
/// </summary>
[TestOf(typeof(PlanetFloorSystem))]
public sealed class PlanetFloorTest : GameTest
{
    [Test]
    public async Task RemovedFloorBecomesSand()
    {
        var server = Pair.Server;
        var entMan = server.EntMan;
        var tileDefs = server.ResolveDependency<ITileDefinitionManager>();
        var map = await Pair.CreateTestMap();
        var mapSys = entMan.System<SharedMapSystem>();
        var sand = tileDefs["F14FloorWastelandSand"].TileId;
        var plating = tileDefs["Plating"].TileId;
        var lattice = tileDefs["Lattice"].TileId;
        var a = new Vector2i(0, 0);
        var b = new Vector2i(1, 0);
        var c = new Vector2i(2, 0);

        await server.WaitPost(() =>
        {
            entMan.AddComponent<PlanetFloorComponent>(map.Grid);
            mapSys.SetTile(map.Grid, a, new Tile(plating));
            mapSys.SetTile(map.Grid, b, new Tile(plating));
            mapSys.SetTile(map.Grid, c, new Tile(sand));
        });
        await server.WaitRunTicks(1);

        await server.WaitPost(() =>
        {
            mapSys.SetTile(map.Grid, a, Tile.Empty);        // pried / exploded to nothing
            mapSys.SetTile(map.Grid, b, new Tile(lattice)); // plating pried to lattice
            mapSys.SetTile(map.Grid, c, Tile.Empty);        // the biome unloading its own sand
        });
        await server.WaitRunTicks(2);

        await server.WaitAssertion(() =>
        {
            Assert.That(mapSys.GetTileRef(map.Grid, a).Tile.TypeId, Is.EqualTo(sand), "removed floor should become sand");
            Assert.That(mapSys.GetTileRef(map.Grid, b).Tile.TypeId, Is.EqualTo(sand), "lattice should become sand");
            Assert.That(mapSys.GetTileRef(map.Grid, c).Tile.IsEmpty, "sand removed by the biome stays unloaded");
        });
    }
}
