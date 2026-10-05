// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_Misfits/Sound/AggroSoundComponent.cs

using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._Misfits.Sound;

/// <summary>
/// Plays a sound when this entity enters combat or attacks, then waits a random cooldown before it can play
/// again. Keeps aggro vocalizations separate from idle sounds.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AggroSoundComponent : Component
{
    /// <summary>
    /// Sound to play when entering combat.
    /// </summary>
    [DataField(required: true)]
    public SoundSpecifier Sound = default!;

    /// <summary>
    /// Minimum seconds between plays. The cooldown is random between this and <see cref="CooldownMax"/>
    /// so mobs in a group don't all vocalize in sync.
    /// </summary>
    [DataField]
    public float CooldownMin = 10f;

    /// <summary>
    /// Maximum seconds between plays.
    /// </summary>
    [DataField]
    public float CooldownMax = 15f;

    /// <summary>
    /// Time remaining before the sound can play again.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float CooldownRemaining;
}
