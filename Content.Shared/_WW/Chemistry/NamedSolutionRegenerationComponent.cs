using Content.Shared.Chemistry.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._WW.Chemistry;

/// <summary>
/// Passively refills one named solution (e.g. a flamer's "chamber" or a venom gland).
/// Wizden's SolutionRegeneration only refills the entity's own solution, which mobs can't use.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
[Access(typeof(NamedSolutionRegenerationSystem))]
public sealed partial class NamedSolutionRegenerationComponent : Component
{
    /// <summary>
    /// The solution to refill.
    /// </summary>
    [DataField(required: true)]
    public string Solution = string.Empty;

    /// <summary>
    /// What gets added each cycle.
    /// </summary>
    [DataField(required: true)]
    public Solution Generated = default!;

    /// <summary>
    /// Time between refills.
    /// </summary>
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(1);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField, AutoNetworkedField]
    public TimeSpan NextRegenTime;
}
