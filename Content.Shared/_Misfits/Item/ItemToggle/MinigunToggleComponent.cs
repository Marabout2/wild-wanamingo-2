// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/Item/ItemToggle/Components/MinigunToggleComponent.cs
// Changed for Wild Wanamingo: moved to _Misfits.

namespace Content.Shared._Misfits.Item.ItemToggle;

/// <summary>
/// Handles changes to GunComponent when the item is toggled.
/// </summary>
[RegisterComponent, Access(typeof(MinigunToggleSystem))]
public sealed partial class MinigunToggleComponent : Component
{
    /// <summary>
    /// fire rate when the gun is "inactive"
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("InactiveWeaponFireRate")]
    public float InactiveWeaponFireRate = 1f;

    /// <summary>
    /// speed modifier applied when the gun is "active"
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("ActivatedSpeedModifier")]
    public float ActivatedSpeedModifier = 0.1f;

    /// <summary>
    /// fire rate when the gun is "active"
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("ActivatedFireRate")]
    public float ActivatedFireRate = 8f;

}
