// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_Misfits/Fire/TileFireComponent.cs
// Changed for Wild Wanamingo: TileFireLayers lives here (Misfits took it from its RMC stubs).

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Misfits.Fire;

/// <summary>
/// A short-lived fire on one tile, left by flamers and molotovs. Ignites and burns whatever stands in it;
/// water vapor puts it out.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class TileFireComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool ExtinguishInstantly = true;

    [DataField, AutoNetworkedField]
    public float VaporExtinguishSeconds = 2f;

    [DataField, AutoNetworkedField]
    public float FireStacks = 1.5f;

    [DataField, AutoNetworkedField]
    public float StandingDamage = 1f;

    [DataField, AutoNetworkedField]
    public TimeSpan TouchCooldown = TimeSpan.FromSeconds(0.5f);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan SpawnedAt;

    [DataField, AutoNetworkedField]
    public TimeSpan Duration = TimeSpan.FromSeconds(20);

    [DataField, AutoNetworkedField]
    public TimeSpan BigFireDuration = TimeSpan.FromSeconds(0.5f);
}

[Serializable, NetSerializable]
public enum TileFireLayers : byte
{
    Base,
}

[Serializable, NetSerializable]
public enum TileFireVisuals : byte
{
    One,
    Two,
    Three,
    Four,
}
