// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_Misfits/Weapons/Ranged/FoldableStockComponent.cs

using Content.Shared.Item;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Misfits.Weapons.Ranged;

/// <summary>
/// Changes a firearm's carrying size and recoil when its stock is folded.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class FoldableStockComponent : Component
{
    [DataField]
    public ProtoId<ItemSizePrototype> FoldedSize = "Normal";

    [DataField]
    public ProtoId<ItemSizePrototype> UnfoldedSize = "Large";

    [DataField]
    public float FoldedRecoilMultiplier = 0.8f;
}
