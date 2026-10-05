// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_Misfits/Weapons/Ranged/FoldableAmmoBoxComponent.cs

using Content.Shared.Materials;
using Robust.Shared.Prototypes;

namespace Content.Shared._Misfits.Weapons.Ranged;

/// <summary>
/// An empty cardboard ammo box that can be folded flat back into crafting material.
/// </summary>
[RegisterComponent]
public sealed partial class FoldableAmmoBoxComponent : Component
{
    [DataField]
    public ProtoId<MaterialPrototype> RefundMaterial = "Cardboard";

    [DataField]
    public int RefundAmount = 100;
}
