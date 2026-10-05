// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_Misfits/Weapons/Ranged/ManualActionComponent.cs

using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._Misfits.Weapons.Ranged;

/// <summary>
/// Requires the firearm to be manually cycled between shots (bolt, lever or pump action).
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ManualActionComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool NeedsCycle;

    /// <summary>
    /// Misfits let you hold the cycle key and click to slam-fire. Kept so prototypes load; it has no effect until a
    /// cycle keybind is added.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool SlamFire;

    [DataField]
    public SoundSpecifier CycleSound = new SoundPathSpecifier("/Audio/Weapons/Guns/Cock/ltrifle_cock.ogg");
}
