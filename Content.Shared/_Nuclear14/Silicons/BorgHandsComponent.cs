// Ported from Nuclear 14 (https://github.com/Vault-Overseers/nuclear-14) via Misfits (https://github.com/Misfit-Sanctuary/nuclear-14):
// Content.Shared/_Nuclear14/Silicons/BorgHandsComponent.cs

namespace Content.Shared._Nuclear14.Silicons;

/// <summary>
/// Gives a robot chassis this many generic hands on map init.
/// </summary>
[RegisterComponent]
public sealed partial class BorgHandsComponent : Component
{
    [DataField]
    public int HandCount = 2;
}
