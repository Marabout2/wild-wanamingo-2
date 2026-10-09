// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_Misfits/Silicon/RobotCriticalSparkingComponent.cs

using Robust.Shared.Audio;

namespace Content.Shared._Misfits.Silicon;

/// <summary>
/// Throws sparks and crackles while the robot is close to destruction; stops once it is repaired.
/// </summary>
[RegisterComponent]
public sealed partial class RobotCriticalSparkingComponent : Component
{
    /// <summary>
    /// Fraction of the death threshold at which sparking starts.
    /// </summary>
    [DataField]
    public float ActivationThreshold = 0.75f;

    /// <summary>
    /// Seconds between spark attempts.
    /// </summary>
    [DataField]
    public float CycleDelay = 1.5f;

    [DataField]
    public TimeSpan SparkCooldown = TimeSpan.FromSeconds(3);

    [DataField]
    public SoundSpecifier Sound = new SoundCollectionSpecifier("sparks");

    [ViewVariables]
    public TimeSpan LastSparkTime;

    [ViewVariables]
    public float AccumulatedFrametime;
}
