// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_Misfits/Deathclaw/BwonsamdiComponent.cs
// Changed for Wild Wanamingo: only the shared marker (emote whitelists); Bwonsamdi's loa abilities aren't ported yet.

using Robust.Shared.GameStates;

namespace Content.Shared._Misfits.Deathclaw;

/// <summary>
/// Shared player-QoL marker for both ordinary sentient Deathclaws and Bwonsamdi.
/// It must never be used as authorization for Bwonsamdi's supernatural abilities.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SentientDeathclawComponent : Component;
