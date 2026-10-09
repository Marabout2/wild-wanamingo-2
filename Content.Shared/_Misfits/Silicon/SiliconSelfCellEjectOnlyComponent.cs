// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_Misfits/Silicon/SiliconSelfCellEjectOnlyComponent.cs

namespace Content.Shared._Misfits.Silicon;

/// <summary>
/// Only the robot itself can take the power cell out of this slot.
/// </summary>
[RegisterComponent]
public sealed partial class SiliconSelfCellEjectOnlyComponent : Component
{
    [DataField]
    public string SlotId = "cell_slot";
}
