// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_Misfits/Wielding/GrantActionOnWieldComponent.cs

using Robust.Shared.Prototypes;

namespace Content.Shared._Misfits.Wielding;

/// <summary>
/// Gives the wielder these actions while the item is wielded.
/// </summary>
[RegisterComponent]
public sealed partial class GrantActionOnWieldComponent : Component
{
    [DataField]
    public List<EntProtoId> Actions = new();

    /// <summary>
    /// The action entities created the first time, reused on later wields and removed on unwield.
    /// </summary>
    public List<EntityUid> ActionIds = new();
}
