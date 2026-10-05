// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_Misfits/Weapons/Sears/FirearmSearComponents.cs

using Content.Shared.DoAfter;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Misfits.Weapons.Sears;

/// <summary>
/// Which permanent sear upgrades a firearm accepts.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FirearmSearCompatibleComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool AllowBurst;

    [DataField, AutoNetworkedField]
    public bool AllowFullAuto;
}

/// <summary>
/// An item used up to permanently add a fire mode to a compatible firearm.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FirearmSearComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public SelectiveFire Mode;

    [DataField, AutoNetworkedField]
    public TimeSpan InstallTime = TimeSpan.FromSeconds(5);
}

/// <summary>
/// The sear installed in a firearm.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class InstalledFirearmSearComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public SelectiveFire Mode;
}

[Serializable, NetSerializable]
public sealed partial class InstallFirearmSearDoAfterEvent : SimpleDoAfterEvent;
