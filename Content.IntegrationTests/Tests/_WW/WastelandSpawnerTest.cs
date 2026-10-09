using System.Linq;
using Content.IntegrationTests.Fixtures;
using Content.Server._WW.Spawners;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.IntegrationTests.Tests._WW;

/// <summary>
/// Wasteland timed spawners only spawn with a living player in range (but not in sight), and stop at their cap.
/// </summary>
[TestFixture, TestOf(typeof(WastelandSpawnerSystem))]
public sealed class WastelandSpawnerTest : GameTest
{
    [TestPrototypes]
    private const string Prototypes = @"
- type: entity
  id: F14TestWastelandSpawner
  components:
  - type: WastelandSpawner
    prototypes: [ MobMouse ]
    intervalSeconds: 1
    activationRange: 30
    spawnBlockRange: 10
    maxAlivePerPrototype: 2
";

    private const string Mob = "MobMouse";

    [Test]
    public async Task SpawnsOnlyWithPlayerInRangeAndRespectsCap()
    {
        var server = Pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();
        var mapSys = server.System<SharedMapSystem>();
        var xformSys = server.System<SharedTransformSystem>();
        MapId map = default;
        EntityUid player = default;

        await server.WaitPost(() =>
        {
            mapSys.CreateMap(out map);
            entMan.SpawnEntity("F14TestWastelandSpawner", new MapCoordinates(0, 0, map));
        });

        // Nobody around: dormant.
        await Pair.RunSeconds(3f);
        await server.WaitAssertion(() => Assert.That(Count(entMan, Mob, map), Is.Zero, "Spawned with no player in range"));

        // A player too close blocks spawning (no pop-in on screen).
        await server.WaitPost(() =>
        {
            player = entMan.SpawnEntity("MobHuman", new MapCoordinates(5, 0, map));
            server.PlayerMan.SetAttachedEntity(server.PlayerMan.Sessions.Single(), player);
        });
        await Pair.RunSeconds(3f);
        await server.WaitAssertion(() => Assert.That(Count(entMan, Mob, map), Is.Zero, "Spawned in sight of a player"));

        // In range but out of sight: spawns, up to the cap of 2.
        await server.WaitPost(() => xformSys.SetWorldPosition(player, new System.Numerics.Vector2(20, 0)));
        await Pair.RunSeconds(8f);
        await server.WaitAssertion(() =>
        {
            var count = Count(entMan, Mob, map);
            Assert.That(count, Is.GreaterThan(0), "Never spawned with a player in range");
            Assert.That(count, Is.LessThanOrEqualTo(2), "Spawned past its alive cap");
            entMan.DeleteEntity(mapSys.GetMap(map));
        });
    }

    private static int Count(IEntityManager entMan, string proto, MapId map)
    {
        return entMan.AllEntities<MetaDataComponent>()
            .Count(e => e.Comp.EntityPrototype?.ID == proto && entMan.GetComponent<TransformComponent>(e).MapID == map);
    }
}
