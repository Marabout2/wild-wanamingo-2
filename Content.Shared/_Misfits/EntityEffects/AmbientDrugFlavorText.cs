// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Server/_Misfits/EntityEffects/Effects/DrugAmbience/AmbientDrugFlavorText.cs
// Changed for Wild Wanamingo: rewritten for Wizden's EntityEffect API (effect here, server system sends the text).

using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared._Misfits.EntityEffects;

/// <summary>
/// Privately sends the player one of <see cref="MessageKeys"/> in local chat (no speaker) while a drug metabolizes,
/// at most once per <see cref="Cooldown"/> seconds.
/// </summary>
public sealed partial class AmbientDrugFlavorText : EntityEffectBase<AmbientDrugFlavorText>
{
    [DataField(required: true)]
    public List<LocId> MessageKeys = new();

    /// <summary>
    /// Effects sharing an id share a cooldown. Defaults to one per message set.
    /// </summary>
    [DataField]
    public string CooldownId = string.Empty;

    [DataField]
    public float Cooldown = 15f;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) => null;
}
