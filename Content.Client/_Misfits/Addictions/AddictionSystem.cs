// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Client/_Misfits/Addictions/AddictionSystem.cs (Misfits ported it from Delta-V, https://github.com/DeltaV-Station/Delta-v)

// #Misfits Change - Ported from Delta-V addiction system
using Content.Shared._Misfits.Addictions;

namespace Content.Client._Misfits.Addictions;

/// <summary>
///     Client-side addiction system stub. All logic is server-side.
/// </summary>
public sealed partial class AddictionSystem : SharedAddictionSystem
{
    protected override void UpdateTime(EntityUid uid) { }
}
