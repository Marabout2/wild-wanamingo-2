// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_Misfits/Silicon/RobotSelfRepairAnnouncerComponent.cs

namespace Content.Shared._Misfits.Silicon;

/// <summary>
/// The robot calls out hull damage when hit hard and announces when its self-repair kicks in.
/// </summary>
[RegisterComponent]
public sealed partial class RobotSelfRepairAnnouncerComponent : Component
{
    /// <summary>
    /// Only hits at least this big trigger the hull warning.
    /// </summary>
    [DataField]
    public float DamageDeltaThreshold = 10f;

    /// <summary>
    /// No hull warning until total damage reaches this.
    /// </summary>
    [DataField]
    public float MinTotalDamageForHullWarning = 30f;

    [DataField]
    public TimeSpan HullWarningCooldown = TimeSpan.FromSeconds(20);

    /// <summary>
    /// Healing only gets announced while total damage is at least this.
    /// </summary>
    [DataField]
    public float MinDamageForRepairPopup = 20f;

    [DataField]
    public TimeSpan RepairAnnounceCooldown = TimeSpan.FromSeconds(30);

    public TimeSpan? NextHullWarningTime;
    public TimeSpan? NextRepairAnnounceTime;
}
