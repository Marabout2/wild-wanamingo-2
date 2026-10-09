// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Server/_Misfits/SpecialStats/SpecialPenaltySystem.cs

using Content.Server._Misfits.SpecialStats.Components;
using Content.Server._Nuclear14.Special.Speech.Components;
using Content.Shared._Misfits.Special;
using Content.Shared._Misfits.Special.Components;
using Content.Shared._Misfits.SpecialStats;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Prototypes;

namespace Content.Server._Misfits.SpecialStats;

/// <summary>
/// Applies negative SPECIAL side effects that are represented by components.
/// </summary>
public sealed partial class SpecialPenaltySystem : EntitySystem
{
    [Dependency] private SharedSpecialSystem _special = default!;
    [Dependency] private StatusEffectsSystem _status = default!;

    private const int LowCharismaThreshold = 5;
    private const int LowIntelligenceThreshold = 2;
    private static readonly EntProtoId LuckClumsyEffect = "F14StatusEffectClumsyLuck";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpecialChangedEvent>(OnSpecialChanged);
        SubscribeLocalEvent<SpecialStatsReadyEvent>(OnStatsReady);
        SubscribeLocalEvent<SpecialShutdownEvent>(OnSpecialShutdown);
    }

    private void OnSpecialChanged(ref SpecialChangedEvent args)
    {
        if (TryComp<SpecialComponent>(args.ChangedEntity, out var special))
            ApplyPenalties((args.ChangedEntity, special));
    }

    private void OnStatsReady(ref SpecialStatsReadyEvent args)
    {
        if (TryComp<SpecialComponent>(args.Entity, out var special))
            ApplyPenalties((args.Entity, special));
    }

    private void OnSpecialShutdown(ref SpecialShutdownEvent args)
    {
        ClearLowCharisma(args.Entity);
        ClearLowIntelligence(args.Entity);
        ClearLuckClumsy(args.Entity);
    }

    private void ApplyPenalties(Entity<SpecialComponent> ent)
    {
        var charisma = _special.GetEffective(ent.Owner, SpecialStat.Charisma, ent.Comp);
        if (charisma < LowCharismaThreshold)
            ApplyLowCharisma(ent.Owner, charisma);
        else
            ClearLowCharisma(ent.Owner);

        if (_special.GetEffective(ent.Owner, SpecialStat.Intelligence, ent.Comp) < LowIntelligenceThreshold)
            ApplyLowIntelligence(ent.Owner);
        else
            ClearLowIntelligence(ent.Owner);

        if (_special.GetEffective(ent.Owner, SpecialStat.Luck, ent.Comp) == 1)
            ApplyLuckClumsy(ent.Owner);
        else
            ClearLuckClumsy(ent.Owner);
    }

    private void ApplyLowCharisma(EntityUid uid, int charisma)
    {
        var comp = EnsureComp<SpecialLowCharismaComponent>(uid);
        comp.Charisma = charisma;
    }

    private void ClearLowCharisma(EntityUid uid)
    {
        RemComp<SpecialLowCharismaComponent>(uid);
    }

    private void ApplyLowIntelligence(EntityUid uid)
    {
        EnsureComp<LowIntelligenceAccentComponent>(uid);
    }

    private void ClearLowIntelligence(EntityUid uid)
    {
        RemComp<LowIntelligenceAccentComponent>(uid);
    }

    // Changed for Wild Wanamingo: Wizden's clumsiness is a status effect now; Luck 1 gets its own mild one.
    private void ApplyLuckClumsy(EntityUid uid)
    {
        if (_status.HasStatusEffect(uid, LuckClumsyEffect))
            return;

        _status.TrySetStatusEffectDuration(uid, LuckClumsyEffect);
    }

    private void ClearLuckClumsy(EntityUid uid)
    {
        _status.TryRemoveStatusEffect(uid, LuckClumsyEffect);
    }
}
