using Content.Shared.Nutrition.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._WW.Nutrition;

/// <summary>
/// Raised on an entity whenever its rate of change for a satiation (hunger, thirst...) is recalculated.
/// Handlers multiply <see cref="Multiplier"/> to make that satiation drain faster or slower.
/// </summary>
[ByRefEvent]
public record struct SatiationChangeRateModifierEvent(ProtoId<SatiationTypePrototype> Type, float Multiplier = 1f);
