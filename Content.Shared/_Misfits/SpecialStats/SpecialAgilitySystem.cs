// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_Misfits/SpecialStats/SpecialAgilitySystem.cs

using Content.Shared._Misfits.Special;
using Content.Shared._Misfits.Special.Components;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Shared._Misfits.SpecialStats;

/// <summary>
/// Applies Agility to close-quarters action cadence.
/// </summary>
public sealed partial class SpecialAgilitySystem : EntitySystem
{
    [Dependency] private SharedSpecialSystem _special = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MeleeWeaponComponent, GetMeleeAttackRateEvent>(OnMeleeAttackRate);
    }

    private void OnMeleeAttackRate(EntityUid uid, MeleeWeaponComponent component, ref GetMeleeAttackRateEvent args)
    {
        if (!TryComp<SpecialComponent>(args.User, out var special))
            return;

        args.Multipliers *= GetActionDelayMultiplier(args.User, special);
    }

    private float GetActionDelayMultiplier(EntityUid uid, SpecialComponent special)
    {
        var tuning = _special.GetTuning();
        var modifier = _special.GetCurvedEffectModifier(
            uid,
            SpecialStat.Agility,
            -tuning.AgilityActionDelayMultiplierPerPoint,
            special);

        return MathF.Max(0.1f, 1f + modifier);
    }
}
