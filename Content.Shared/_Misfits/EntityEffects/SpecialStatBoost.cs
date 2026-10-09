// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Server/_Misfits/EntityEffects/Effects/SpecialStatBoostEffect.cs
// Changed for Wild Wanamingo: rewritten for Wizden's EntityEffectSystem API.

using Content.Shared._Misfits.SpecialStats;
using Content.Shared.EntityEffects;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared._Misfits.EntityEffects;

/// <summary>
/// Drug stat boost (Buffout, Mentats, Psycho...) while the chem is in the system: flat bonuses through
/// <see cref="DrugSpecialBoostComponent"/>, refreshed each metabolism tick.
/// </summary>
public sealed partial class SpecialStatBoost : EntityEffectBase<SpecialStatBoost>
{
    [DataField] public int StrengthBoost;
    [DataField] public int PerceptionBoost;
    [DataField] public int EnduranceBoost;
    [DataField] public int CharismaBoost;
    [DataField] public int AgilityBoost;
    [DataField] public int IntelligenceBoost;
    [DataField] public int LuckBoost;

    /// <summary>
    /// Seconds the boost lasts after the last tick.
    /// </summary>
    [DataField]
    public float StatusLifetime = 4f;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        var parts = new List<string>();
        if (StrengthBoost != 0) parts.Add($"STR +{StrengthBoost}");
        if (PerceptionBoost != 0) parts.Add($"PER +{PerceptionBoost}");
        if (EnduranceBoost != 0) parts.Add($"END +{EnduranceBoost}");
        if (CharismaBoost != 0) parts.Add($"CHA +{CharismaBoost}");
        if (AgilityBoost != 0) parts.Add($"AGI +{AgilityBoost}");
        if (IntelligenceBoost != 0) parts.Add($"INT +{IntelligenceBoost}");
        if (LuckBoost != 0) parts.Add($"LCK +{LuckBoost}");

        return parts.Count > 0
            ? Loc.GetString("reagent-effect-guidebook-special-boost", ("chance", Probability), ("stats", string.Join(", ", parts)))
            : null;
    }
}

public sealed partial class SpecialStatBoostEntityEffectSystem : EntityEffectSystem<MobStateComponent, SpecialStatBoost>
{
    [Dependency] private DrugSpecialBoostSystem _boost = default!;
    [Dependency] private MovementSpeedModifierSystem _movement = default!;

    protected override void Effect(Entity<MobStateComponent> entity, ref EntityEffectEvent<SpecialStatBoost> args)
    {
        var effect = args.Effect;
        var comp = EnsureComp<DrugSpecialBoostComponent>(entity);

        if (effect.StrengthBoost != 0) comp.StrengthBoost = effect.StrengthBoost;
        if (effect.PerceptionBoost != 0) comp.PerceptionBoost = effect.PerceptionBoost;
        if (effect.EnduranceBoost != 0) comp.EnduranceBoost = effect.EnduranceBoost;
        if (effect.CharismaBoost != 0) comp.CharismaBoost = effect.CharismaBoost;
        if (effect.AgilityBoost != 0) comp.AgilityBoost = effect.AgilityBoost;
        if (effect.IntelligenceBoost != 0) comp.IntelligenceBoost = effect.IntelligenceBoost;
        if (effect.LuckBoost != 0) comp.LuckBoost = effect.LuckBoost;

        _boost.RefreshTimer(entity, comp, effect.StatusLifetime);

        if (effect.AgilityBoost != 0)
            _movement.RefreshMovementSpeedModifiers(entity.Owner);

        Dirty(entity, comp);
    }
}
