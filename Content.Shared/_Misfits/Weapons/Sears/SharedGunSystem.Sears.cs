// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): AddFireMode from Content.Shared/Weapons/Ranged/Systems/SharedGunSystem.cs
// Kept as a partial of Wizden's SharedGunSystem in our folder, since GunComponent can only be written by that system.

using Content.Shared.Weapons.Ranged.Components;

namespace Content.Shared.Weapons.Ranged.Systems;

public abstract partial class SharedGunSystem
{
    /// <summary>
    /// Permanently adds a fire mode to a gun, e.g. when a sear is installed.
    /// </summary>
    public void AddFireMode(EntityUid uid, SelectiveFire mode, GunComponent? component = null)
    {
        if (!Resolve(uid, ref component) || (component.AvailableModes & mode) != 0)
            return;

        component.AvailableModes |= mode;
        Dirty(uid, component);
    }
}
