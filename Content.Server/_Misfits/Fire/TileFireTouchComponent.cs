// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Server/_Misfits/Fire/TileFireTouchComponent.cs

using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._Misfits.Fire;

/// <summary>
/// Rate-limits how often tile fires burn an entity.
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class TileFireTouchComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextTouchAt;
}
