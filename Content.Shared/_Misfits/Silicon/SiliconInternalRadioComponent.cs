// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_Misfits/Silicon/SiliconInternalRadioComponent.cs

namespace Content.Shared._Misfits.Silicon;

/// <summary>
/// Built-in radio for player robots: only works while the robot is up, and the department prefix uses the
/// default channel of the keys in its own key holder.
/// </summary>
[RegisterComponent]
public sealed partial class SiliconInternalRadioComponent : Component;
