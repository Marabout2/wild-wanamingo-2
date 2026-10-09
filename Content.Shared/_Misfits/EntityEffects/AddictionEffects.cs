// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Server/_Misfits/EntityEffects/Effects/Addiction/*.cs
// (Misfits ported it from Delta-V, https://github.com/DeltaV-Station/Delta-v)
// Changed for Wild Wanamingo: rewritten for Wizden's EntityEffectSystem API. Wizden's effect events no longer say which
// reagent fired them, so the porter writes the reagent into `reagent:`. Withdrawal moodlets are gone (no mood), and
// CureAddiction is new (Misfits' Fixer referenced it, but it never existed).

using Content.Shared._Misfits.Addictions;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage;
using Content.Shared.EntityEffects;
using Content.Shared.StatusEffect;
using Robust.Shared.Prototypes;

namespace Content.Shared._Misfits.EntityEffects;

/// <summary>
/// Applies (or builds toward) an addiction to <see cref="Reagent"/>. Duration scales with the metabolized amount.
/// </summary>
public sealed partial class Addicting : EntityEffectBase<Addicting>
{
    /// <summary>
    /// The drug causing it (filled in by the porter from the reagent this effect belongs to).
    /// </summary>
    [DataField]
    public ProtoId<ReagentPrototype>? Reagent;

    /// <summary>
    /// Base addiction time in seconds per 1u metabolized.
    /// </summary>
    [DataField]
    public float Time = 5f;

    /// <summary>
    /// Damage applied periodically during withdrawal.
    /// </summary>
    [DataField]
    public DamageSpecifier? WithdrawalDamage;

    /// <summary>
    /// Movement speed multiplier during withdrawal (1 = none).
    /// </summary>
    [DataField]
    public float WithdrawalSpeedPenalty = 1f;

    /// <summary>
    /// Stamina damage applied periodically during withdrawal.
    /// </summary>
    [DataField]
    public float WithdrawalStaminaDrain;

    /// <summary>
    /// Exposures to this drug before a full addiction sets in.
    /// </summary>
    [DataField]
    public int AddictionThreshold = 4;

    public override string EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-addicted", ("chance", Probability));
}

/// <summary>
/// Suppresses withdrawal symptoms for a while. Duration scales with the metabolized amount.
/// </summary>
public sealed partial class SuppressAddiction : EntityEffectBase<SuppressAddiction>
{
    /// <summary>
    /// Base suppression time in seconds per 1u metabolized.
    /// </summary>
    [DataField]
    public float Time = 30f;

    public override string EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-addiction-suppression", ("chance", Probability));
}

/// <summary>
/// Ends all addictions (Fixer).
/// </summary>
public sealed partial class CureAddiction : EntityEffectBase<CureAddiction>
{
    public override string EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-addiction-cure", ("chance", Probability));
}

public sealed partial class AddictingEntityEffectSystem : EntityEffectSystem<StatusEffectsComponent, Addicting>
{
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private SharedAddictionSystem _addiction = default!;

    protected override void Effect(Entity<StatusEffectsComponent> entity, ref EntityEffectEvent<Addicting> args)
    {
        var effect = args.Effect;
        var name = effect.Reagent is { } id && _proto.TryIndex(id, out var reagent) ? reagent.LocalizedName : string.Empty;

        if (!_addiction.TryApplyAddiction(entity, effect.Time * args.Scale, effect.Reagent, name, effect.AddictionThreshold,
                status: entity.Comp))
        {
            return;
        }

        _addiction.SetWithdrawalEffects(entity, string.Empty, effect.WithdrawalDamage, effect.WithdrawalSpeedPenalty,
            effect.WithdrawalStaminaDrain);
    }
}

public sealed partial class SuppressAddictionEntityEffectSystem : EntityEffectSystem<AddictedComponent, SuppressAddiction>
{
    [Dependency] private SharedAddictionSystem _addiction = default!;

    protected override void Effect(Entity<AddictedComponent> entity, ref EntityEffectEvent<SuppressAddiction> args)
    {
        _addiction.TrySuppressAddiction(entity, args.Effect.Time * args.Scale);
    }
}

public sealed partial class CureAddictionEntityEffectSystem : EntityEffectSystem<AddictedComponent, CureAddiction>
{
    protected override void Effect(Entity<AddictedComponent> entity, ref EntityEffectEvent<CureAddiction> args)
    {
        RemCompDeferred<AddictedComponent>(entity);
        // Clean slate: exposures counted toward the next addiction are cleared too.
        RemCompDeferred<AddictionExposureComponent>(entity);
    }
}
