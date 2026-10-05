// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/Weapons/Ranged/Components/GunDamageModifierComponent.cs
// Moved into _Misfits; applied by GunDamageModifierSystem instead of an edit to Wizden's gun system.

using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._Misfits.Weapons;

/// <summary>
/// Adds extra damage to every projectile this gun fires.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class GunDamageModifierComponent : Component
{
    [DataField]
    public DamageSpecifier Damage = new();
}
