// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Server/_Misfits/Silicon/RobotCriticalSparkingSystem.cs

using Content.Shared._Misfits.Silicon;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Server._Misfits.Silicon;

public sealed partial class RobotCriticalSparkingSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private MobThresholdSystem _mobThreshold = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private SharedAudioSystem _audio = default!;

    // Sparks only fire every CycleDelay, so scanning at 10 Hz is plenty.
    private const float UpdateInterval = 0.1f;
    private float _accumTimer;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _accumTimer += frameTime;
        if (_accumTimer < UpdateInterval)
            return;

        var dt = _accumTimer;
        _accumTimer = 0f;

        var query = EntityQueryEnumerator<RobotCriticalSparkingComponent, MobStateComponent, MobThresholdsComponent, DamageableComponent>();
        while (query.MoveNext(out var uid, out var comp, out var mobState, out var thresholds, out var damageable))
        {
            if (_mobState.IsDead(uid, mobState)
                || !_mobThreshold.TryGetThresholdForState(uid, MobState.Dead, out var deathThreshold, thresholds)
                || _damageable.GetTotalDamage((uid, damageable)) < deathThreshold * comp.ActivationThreshold)
            {
                comp.AccumulatedFrametime = 0f;
                continue;
            }

            comp.AccumulatedFrametime += dt;
            if (comp.AccumulatedFrametime < comp.CycleDelay)
                continue;

            comp.AccumulatedFrametime -= comp.CycleDelay;
            if (_timing.CurTime <= comp.LastSparkTime + comp.SparkCooldown)
                continue;

            comp.LastSparkTime = _timing.CurTime;
            Spawn("EffectSparks", Transform(uid).Coordinates);
            _audio.PlayPvs(comp.Sound, uid, AudioParams.Default.WithVariation(0.05f));
        }
    }
}
