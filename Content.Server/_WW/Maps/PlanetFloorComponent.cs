using Content.Shared.Maps;
using Robust.Shared.Prototypes;

namespace Content.Server._WW.Maps;

/// <summary>
/// On a planet grid, any tile that turns into space (prying, explosions, RCDs, admin tools...) becomes
/// <see cref="Fallback"/> instead, so the wasteland never shows vacuum.
/// </summary>
[RegisterComponent]
public sealed partial class PlanetFloorComponent : Component
{
    [DataField]
    public ProtoId<ContentTileDefinition> Fallback = "F14FloorWastelandSand";

    /// <summary>
    /// Tiles waiting to be refilled (tiles can't be set from inside a tile-changed event).
    /// </summary>
    [ViewVariables]
    public HashSet<Vector2i> Pending = new();
}
