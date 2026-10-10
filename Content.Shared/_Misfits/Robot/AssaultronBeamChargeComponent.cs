// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_Misfits/Robot/AssaultronBeamChargeComponent.cs
// Changed for Wild Wanamingo: the battery drain is gone (Misfits drained a cell slot the gun never had; every
// prototype set it to 0) and the charge logic lives in ActionGunSystem alone.

namespace Content.Shared._Misfits.Robot;

/// <summary>
/// Adds a charge-up phase before an action gun fires: the first use starts charging, the next use once charged
/// fires, then the weapon cools down.
/// </summary>
[RegisterComponent]
public sealed partial class AssaultronBeamChargeComponent : Component
{
    /// <summary>
    /// How long the charge-up lasts before the weapon can fire.
    /// </summary>
    [DataField]
    public float ChargeDuration = 2f;

    /// <summary>
    /// Cooldown after a shot before charging can begin again.
    /// </summary>
    [DataField]
    public float CooldownDuration = 3f;

    /// <summary>
    /// Emote said when charging starts.
    /// </summary>
    [DataField]
    public LocId ChargeEmoteLocale = "assaultron-charge-emote";

    /// <summary>
    /// Emote said when the weapon fires.
    /// </summary>
    [DataField]
    public LocId FireEmoteLocale = "assaultron-beam-fire-emote";

    /// <summary>
    /// Minimum seconds between repeated emotes, so rapid use doesn't spam chat.
    /// </summary>
    [DataField]
    public float EmoteCooldown = 20f;

    public bool IsCharging;
    public TimeSpan ChargeEndTime;
    public TimeSpan CooldownEndTime;
    public TimeSpan NextChargeEmoteTime;
    public TimeSpan NextFireEmoteTime;
}
