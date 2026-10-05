// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_Misfits/Weapons/Components/BallisticDamageFalloffComponent.cs

using Robust.Shared.Map;

namespace Content.Shared._Misfits.Weapons.Components;

/// <summary>
/// On a ballistic projectile: linearly reduces its damage based on how far it has travelled from where it was fired.
/// Damage is unaffected before <see cref="FalloffStartTiles"/> and reaches <see cref="MinDamageMultiplier"/> at
/// <see cref="MaxFalloffTiles"/>.
/// </summary>
/// <remarks>
/// Large calibres (e.g. .50 BMG) get a long full-damage range and a high minimum; pistol rounds fall off sooner.
/// Lasers and plasma don't carry this component.
/// </remarks>
[RegisterComponent]
public sealed partial class BallisticDamageFalloffComponent : Component
{
    /// <summary>
    /// Distance in tiles from the spawn point at which damage falloff begins.
    /// </summary>
    [DataField]
    public float FalloffStartTiles = 4f;

    /// <summary>
    /// Distance in tiles at which damage reaches <see cref="MinDamageMultiplier"/>.
    /// </summary>
    [DataField]
    public float MaxFalloffTiles = 15f;

    /// <summary>
    /// The lowest damage multiplier, reached at <see cref="MaxFalloffTiles"/>. Between 0 and 1.
    /// </summary>
    [DataField]
    public float MinDamageMultiplier = 0.5f;

    /// <summary>
    /// Where the projectile entered the map. Set automatically; not for YAML.
    /// </summary>
    public MapCoordinates SpawnPosition = MapCoordinates.Nullspace;
}
