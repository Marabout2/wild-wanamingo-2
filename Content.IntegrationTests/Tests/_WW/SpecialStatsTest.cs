#nullable enable
using Content.IntegrationTests.Fixtures;
using Content.IntegrationTests.Pair;
using Content.Server._Misfits.SpecialStats.Components;
using Content.Server._Nuclear14.Special.Speech.Components;
using Content.Shared._Misfits.Special;
using Content.Shared._Misfits.SpecialStats;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Preferences;
using Content.Shared.StatusEffectNew;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._WW;

/// <summary>
/// A character's SPECIAL stats reach the systems they drive once their profile is applied at spawn.
/// </summary>
[TestOf(typeof(SharedSpecialSystem))]
public sealed class SpecialStatsTest : GameTest
{
    private static readonly EntProtoId LuckClumsy = "F14StatusEffectClumsyLuck";

    private static async Task<(EntityUid Low, EntityUid High)> SpawnPair(TestPair pair, SpecialProfile low, SpecialProfile high)
    {
        var server = pair.Server;
        var entMan = server.EntMan;
        var special = entMan.System<SharedSpecialSystem>();
        var map = await pair.CreateTestMap();
        EntityUid lowUid = default, highUid = default;

        await server.WaitPost(() =>
        {
            lowUid = entMan.SpawnEntity("MobHuman", map.MapCoords);
            highUid = entMan.SpawnEntity("MobHuman", map.MapCoords);

            foreach (var (uid, stats) in new[] { (lowUid, low), (highUid, high) })
            {
                var profile = HumanoidCharacterProfile.DefaultWithSpecies().WithSpecial(stats);
                Assert.That(special.TryApplyProfileBaseValues(uid, profile));
                var ev = new SpecialStatsReadyEvent(uid);
                entMan.EventBus.RaiseLocalEvent(uid, ref ev, true);
            }
        });
        await server.WaitRunTicks(2);
        return (lowUid, highUid);
    }

    [Test]
    public async Task StatsChangeTheBody()
    {
        var pair = Pair;
        var entMan = pair.Server.EntMan;
        var thresholds = entMan.System<MobThresholdSystem>();
        var status = entMan.System<StatusEffectsSystem>();

        // Both spend the same 40 points, in opposite places.
        var weak = new SpecialProfile { Strength = 5, Perception = 5, Endurance = 1, Charisma = 9, Intelligence = 1, Agility = 1, Luck = 9 };
        var strong = new SpecialProfile { Strength = 1, Perception = 1, Endurance = 10, Charisma = 1, Intelligence = 10, Agility = 10, Luck = 1 };
        Assert.That(weak.IsValid && strong.IsValid);

        var (low, high) = await SpawnPair(pair, weak, strong);

        await pair.Server.WaitAssertion(() =>
        {
            // Endurance moves the crit and death thresholds.
            Assert.That(thresholds.GetThresholdForState(high, MobState.Critical),
                Is.GreaterThan(thresholds.GetThresholdForState(low, MobState.Critical)));
            Assert.That(thresholds.GetThresholdForState(high, MobState.Dead),
                Is.GreaterThan(thresholds.GetThresholdForState(low, MobState.Dead)));

            // Agility is movement speed.
            Assert.That(entMan.GetComponent<MovementSpeedModifierComponent>(high).CurrentWalkSpeed,
                Is.GreaterThan(entMan.GetComponent<MovementSpeedModifierComponent>(low).CurrentWalkSpeed));

            // Low stats bring their quirks; high stats don't.
            Assert.That(entMan.HasComponent<LowIntelligenceAccentComponent>(low));
            Assert.That(entMan.HasComponent<LowIntelligenceAccentComponent>(high), Is.False);
            Assert.That(entMan.HasComponent<SpecialLowCharismaComponent>(high));
            Assert.That(entMan.HasComponent<SpecialLowCharismaComponent>(low), Is.False);
            Assert.That(status.HasStatusEffect(high, LuckClumsy));
            Assert.That(status.HasStatusEffect(low, LuckClumsy), Is.False);

            // Intelligence 10 gets the medical HUD.
            Assert.That(entMan.HasComponent<SpecialAppliedMedicalHudComponent>(high));
            Assert.That(entMan.HasComponent<SpecialAppliedMedicalHudComponent>(low), Is.False);
        });
    }
}
