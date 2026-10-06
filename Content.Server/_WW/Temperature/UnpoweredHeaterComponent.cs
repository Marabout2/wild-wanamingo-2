namespace Content.Server._WW.Temperature;

/// <summary>
/// Heats entities placed on this (via <see cref="Content.Shared.Placeable.ItemPlacerComponent"/>) without the
/// power grid: wasteland stoves, propane grills and bonfires. Cooking itself is Wizden's: the heat raises the
/// food's temperature and the temperature construction steps (steaks, cutlets, eggs) do the rest.
/// </summary>
/// <remarks>
/// With an <see cref="Content.Shared.Temperature.Components.EntityHeaterComponent"/> the heater's own
/// off/low/medium/high setting scales <see cref="Power"/>, like the electric grill. Without one it runs at full
/// power whenever it's on (see <see cref="RequireHot"/>).
/// </remarks>
[RegisterComponent, Access(typeof(UnpoweredHeaterSystem))]
public sealed partial class UnpoweredHeaterComponent : Component
{
    /// <summary>
    /// Heat in watts at full setting, shared between everything placed on it.
    /// </summary>
    [DataField]
    public float Power = 12000f;

    /// <summary>
    /// Only heat while the entity is hot (e.g. a lit bonfire). Also toggles its ambient sound and
    /// <see cref="Content.Server.Chemistry.Components.SolutionHeaterComponent"/>, if present.
    /// </summary>
    [DataField]
    public bool RequireHot;
}
