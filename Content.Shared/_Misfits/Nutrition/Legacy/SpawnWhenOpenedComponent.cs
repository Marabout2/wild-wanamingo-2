// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_NC/SpawnWhenOpened/SpawnWhenOpenedComponent.cs
// (Misfits' copy of a Corvax Forge file.)

using Content.Shared.Storage;
using Robust.Shared.GameStates;

namespace Content.Shared._Misfits.Nutrition.Legacy;

/// <summary>
/// Spawns items (e.g. a bottle cap) when the entity is first opened or destroyed.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SpawnWhenOpenedComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool IsAlreadyOpened = false;

    /// <summary>
    /// Can you spawn prototype if you open object multiple times
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsRepeatable = false;

    /// <summary>
    /// The list of prototypes that will spawn when object opened
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<EntitySpawnEntry> Prototypes = new();
}
