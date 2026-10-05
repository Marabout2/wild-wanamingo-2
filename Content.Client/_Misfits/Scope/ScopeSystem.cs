// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Client/_Misfits/Scope/ScopeSystem.cs

// #Misfits Add - Client-side scope system (empty override, shared logic handles prediction)
using Content.Shared._Misfits.Scope;

namespace Content.Client._Misfits.Scope;

/// <summary>
/// Client-side scope system. Inherits all shared prediction logic from
/// <see cref="SharedScopeSystem"/>. No additional client-specific behavior needed —
/// <see cref="ScopingComponent"/> applies eye offset via <see cref="GetEyeOffsetEvent"/>
/// which is handled in the shared system.
/// </summary>
public sealed partial class ScopeSystem : SharedScopeSystem;
