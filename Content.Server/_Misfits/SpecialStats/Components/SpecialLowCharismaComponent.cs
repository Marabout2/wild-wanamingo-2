// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Server/_Misfits/SpecialStats/Components/SpecialLowCharismaComponent.cs

namespace Content.Server._Misfits.SpecialStats.Components;

/// <summary>
/// Applies roleplay-facing low Charisma tells.
/// </summary>
[RegisterComponent]
public sealed partial class SpecialLowCharismaComponent : Component
{
    public int Charisma;
}
