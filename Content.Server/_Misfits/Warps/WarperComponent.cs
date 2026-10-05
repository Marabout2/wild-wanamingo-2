// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Server/Warps/WarperComponent.cs (Misfits' copy of the Space Station 14 warper, rewritten for current Wizden)

namespace Content.Server._Misfits.Warps;

/// <summary>
/// Misfits: ladders, manholes and similar. Using one moves the user to the other <see cref="WarperComponent"/>
/// with the same <see cref="ID"/>, so a ladder pair is set up by giving both ends the same id.
/// </summary>
[RegisterComponent]
public sealed partial class WarperComponent : Component
{
    /// <summary>
    /// Link id shared by both ends.
    /// </summary>
    [DataField("id")]
    public string? ID;
}
