// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_Misfits/SpecialStats/SpecialEnduranceSystem.cs
// Changed for Wild Wanamingo: hunger/thirst drain goes through Wizden's satiation (SatiationChangeRateModifierEvent);
// Wizden has no soft crit, so only the crit and death thresholds move.

using Content.Shared._Misfits.Special;
using Content.Shared._Misfits.Special.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared._WW.Nutrition;
using Content.Shared.Damage.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.Maths;

namespace Content.Shared._Misfits.SpecialStats;

/// <summary>
/// Applies Endurance health bonuses when S.P.E.C.I.A.L. changes.
/// </summary>
public sealed partial class SpecialEnduranceSystem : EntitySystem
{
    [Dependency] private SharedSpecialSystem _special = default!;
    [Dependency] private MobThresholdSystem _thresholds = default!;
    [Dependency] private SatiationSystem _satiation = default!;

    private static readonly MobState[] HealthThresholdStates =
    [
        MobState.Critical,
        MobState.Dead,
    ];

    private static readonly string[] EnduranceResistedDamageTypes =
    [
        "Poison",
        "Radiation",
        "Caustic",
        "Cellular",
    ];

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpecialComponent, SpecialChangedEvent>(OnSpecialChanged);
        SubscribeLocalEvent<SpecialComponent, SpecialStatsReadyEvent>(OnStatsReady);
        SubscribeLocalEvent<SpecialComponent, SpecialShutdownEvent>(OnSpecialShutdown);
        SubscribeLocalEvent<SpecialComponent, DamageModifyEvent>(OnDamageModify);
        SubscribeLocalEvent<SpecialComponent, SatiationChangeRateModifierEvent>(OnSatiationRate);
    }

    private void OnSpecialChanged(Entity<SpecialComponent> ent, ref SpecialChangedEvent args)
    {
        ApplyEndurance(ent, false);
    }

    private void OnStatsReady(Entity<SpecialComponent> ent, ref SpecialStatsReadyEvent args)
    {
        ApplyEndurance(ent, false);
    }

    private void OnSpecialShutdown(Entity<SpecialComponent> ent, ref SpecialShutdownEvent args)
    {
        ApplyEndurance(ent, true);
    }

    private void OnDamageModify(EntityUid uid, SpecialComponent component, DamageModifyEvent args)
    {
        if (!args.Damage.AnyPositive())
            return;

        var tuning = _special.GetTuning();
        var modifier = _special.GetCurvedEffectModifier(
            uid,
            SpecialStat.Endurance,
            -tuning.EnduranceToxinDamageMultiplierPerPoint,
            component);
        var multiplier = MathF.Max(0.1f, 1f + modifier);

        if (MathHelper.CloseTo(multiplier, 1f))
            return;

        var damage = new DamageSpecifier(args.Damage);
        var modified = false;

        foreach (var type in EnduranceResistedDamageTypes)
        {
            if (!damage.DamageDict.TryGetValue(type, out var value) || value <= FixedPoint2.Zero)
                continue;

            damage.DamageDict[type] = value * multiplier;
            modified = true;
        }

        if (modified)
            args.Damage = damage;
    }

    private void ApplyEndurance(Entity<SpecialComponent> ent, bool reset)
    {
        ClearLegacyStaminaModifier(ent);

        ApplyHealthThresholds(ent, reset);
        ApplyNeedDecay(ent, reset);
        ApplyStaminaRecovery(ent, reset);
    }

    private void ApplyHealthThresholds(Entity<SpecialComponent> ent, bool reset)
    {
        if (!TryComp<MobThresholdsComponent>(ent.Owner, out var thresholds))
            return;

        var tuning = _special.GetTuning();
        var desired = reset
            ? 0f
            : _special.GetCurvedEffectModifier(
                ent.Owner,
                SpecialStat.Endurance,
                tuning.EnduranceHealthModifierPerPoint,
                ent.Comp);
        var adjustment = desired - ent.Comp.AppliedHealthThresholdModifier;

        if (MathHelper.CloseTo(adjustment, 0f))
            return;

        foreach (var state in HealthThresholdStates)
        {
            var threshold = _thresholds.GetThresholdForState(ent.Owner, state, thresholds);
            if (threshold == FixedPoint2.Zero)
                continue;

            _thresholds.SetMobStateThreshold(ent.Owner, FixedPoint2.Max(1, threshold + adjustment), state, thresholds);
        }

        ent.Comp.AppliedHealthThresholdModifier = desired;

        Dirty(ent.Owner, ent.Comp);
    }

    private void ApplyNeedDecay(Entity<SpecialComponent> ent, bool reset)
    {
        if (!TryComp<SatiationComponent>(ent.Owner, out var satiation))
            return;

        var desired = reset ? 1f : GetNeedDecayMultiplier(ent);
        var previous = ent.Comp.AppliedHungerDecayMultiplier <= 0f
            ? 1f
            : ent.Comp.AppliedHungerDecayMultiplier;

        if (MathHelper.CloseTo(desired, previous))
            return;

        // The rate is picked up by OnSatiationRate; nudging each satiation makes Wizden recalculate it now.
        ent.Comp.AppliedHungerDecayMultiplier = desired;
        ent.Comp.AppliedThirstDecayMultiplier = desired;
        Dirty(ent.Owner, ent.Comp);
        _satiation.ModifyValue((ent.Owner, satiation), SatiationSystem.Hunger, 0f);
        _satiation.ModifyValue((ent.Owner, satiation), SatiationSystem.Thirst, 0f);
    }

    private void OnSatiationRate(Entity<SpecialComponent> ent, ref SatiationChangeRateModifierEvent args)
    {
        if (args.Type != SatiationSystem.Hunger && args.Type != SatiationSystem.Thirst)
            return;

        var multiplier = ent.Comp.AppliedHungerDecayMultiplier;
        if (multiplier > 0f)
            args.Multiplier *= multiplier;
    }

    private void ApplyStaminaRecovery(Entity<SpecialComponent> ent, bool reset)
    {
        if (!TryComp<StaminaComponent>(ent.Owner, out var stamina))
            return;

        var desired = reset ? 1f : GetStaminaRecoveryMultiplier(ent);
        var previous = ent.Comp.AppliedStaminaRecoveryMultiplier <= 0f
            ? 1f
            : ent.Comp.AppliedStaminaRecoveryMultiplier;

        if (MathHelper.CloseTo(desired, previous))
            return;

        stamina.Decay *= desired / previous;
        ent.Comp.AppliedStaminaRecoveryMultiplier = desired;

        Dirty(ent.Owner, stamina);
        Dirty(ent.Owner, ent.Comp);
    }

    private float GetNeedDecayMultiplier(Entity<SpecialComponent> ent)
    {
        var tuning = _special.GetTuning();
        var modifier = _special.GetCurvedEffectModifier(
            ent.Owner,
            SpecialStat.Endurance,
            -tuning.EnduranceNeedDecayMultiplierPerPoint,
            ent.Comp);

        return MathF.Max(0.1f, 1f + modifier);
    }

    private float GetStaminaRecoveryMultiplier(Entity<SpecialComponent> ent)
    {
        var tuning = _special.GetTuning();
        var modifier = _special.GetCurvedEffectModifier(
            ent.Owner,
            SpecialStat.Endurance,
            tuning.EnduranceStaminaRecoveryMultiplierPerPoint,
            ent.Comp);

        return MathF.Max(0.1f, 1f + modifier);
    }

    private void ClearLegacyStaminaModifier(Entity<SpecialComponent> ent)
    {
        if (MathHelper.CloseTo(ent.Comp.AppliedStaminaCritThresholdModifier, 0f))
            return;

        if (TryComp<StaminaComponent>(ent.Owner, out var stamina))
        {
            stamina.CritThreshold -= ent.Comp.AppliedStaminaCritThresholdModifier;
            Dirty(ent.Owner, stamina);
        }

        ent.Comp.AppliedStaminaCritThresholdModifier = 0f;
        Dirty(ent.Owner, ent.Comp);
    }
}
