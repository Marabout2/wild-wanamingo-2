using Content.Server.Chemistry.Components;
using Content.Server.Chemistry.EntitySystems;
using Content.Server.Temperature.Systems;
using Content.Shared.Audio;
using Content.Shared.Mobs.Components;
using Content.Shared.Placeable;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Temperature;
using Content.Shared.Temperature.Components;

namespace Content.Server._WW.Temperature;

/// <summary>
/// Adds heat to entities placed on an <see cref="UnpoweredHeaterComponent"/>.
/// </summary>
public sealed partial class UnpoweredHeaterSystem : EntitySystem
{
    [Dependency] private SharedAmbientSoundSystem _ambientSound = default!;
    [Dependency] private SolutionHeaterSystem _solutionHeater = default!;
    [Dependency] private TemperatureSystem _temperature = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UnpoweredHeaterComponent, StepTriggerAttemptEvent>(OnStepTriggerAttempt);
    }

    /// <summary>
    /// A fire only burns what walks into it while lit, and only mobs: not food sitting on it.
    /// (Behaviour from Misfits' BonfireHeaterSystem.)
    /// </summary>
    private void OnStepTriggerAttempt(Entity<UnpoweredHeaterComponent> ent, ref StepTriggerAttemptEvent args)
    {
        if (!ent.Comp.RequireHot)
            return;

        var ev = new IsHotEvent();
        RaiseLocalEvent(ent, ev);
        if (!ev.IsHot || !HasComp<MobStateComponent>(args.Tripper))
            args.Cancelled = true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<UnpoweredHeaterComponent, ItemPlacerComponent>();
        while (query.MoveNext(out var uid, out var heater, out var placer))
        {
            if (heater.RequireHot && !UpdateHot(uid, placer))
                continue;

            var power = TryComp<EntityHeaterComponent>(uid, out var setting)
                ? SettingPower(setting.Setting, heater.Power)
                : heater.Power;

            if (power <= 0f || placer.PlacedEntities.Count == 0)
                continue;

            var energy = power * frameTime / placer.PlacedEntities.Count;
            foreach (var ent in placer.PlacedEntities)
            {
                _temperature.ChangeHeat(ent, energy);
            }
        }
    }

    /// <summary>
    /// Syncs ambience and the solution heater with the fire, and returns whether it's lit.
    /// </summary>
    private bool UpdateHot(EntityUid uid, ItemPlacerComponent placer)
    {
        var ev = new IsHotEvent();
        RaiseLocalEvent(uid, ev);

        _ambientSound.SetAmbience(uid, ev.IsHot);
        if (HasComp<SolutionHeaterComponent>(uid))
        {
            if (ev.IsHot)
                _solutionHeater.TryTurnOn(uid, placer);
            else
                _solutionHeater.TurnOff(uid);
        }

        return ev.IsHot;
    }

    /// <summary>
    /// Same scaling as the electric grill's settings.
    /// </summary>
    private static float SettingPower(EntityHeaterSetting setting, float max)
    {
        return setting switch
        {
            EntityHeaterSetting.Low => max / 4f,
            EntityHeaterSetting.Medium => max / 2f,
            EntityHeaterSetting.High => max,
            _ => 0f,
        };
    }
}
