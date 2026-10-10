// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/Weapons/Ranged/Components/ActionGunComponent.cs
// (Misfits' copy of a Space Station 14 file; Wizden has since removed it.)

using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Misfits.Weapons.Legacy;

/// <summary>
/// Lets you shoot a gun using an action (assaultron head laser, tesla emitter).
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ActionGunComponent : Component
{
    /// <summary>
    /// Action to create, must use <see cref="ActionGunShootEvent"/>.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId Action = string.Empty;

    [DataField]
    public EntityUid? ActionEntity;

    /// <summary>
    /// Prototype of gun entity to spawn.
    /// Deleted when this component is removed.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId GunProto = string.Empty;

    [DataField]
    public EntityUid? Gun;
}

/// <summary>
/// Action event for <see cref="ActionGunComponent"/> to shoot at a position.
/// </summary>
public sealed partial class ActionGunShootEvent : WorldTargetActionEvent;
