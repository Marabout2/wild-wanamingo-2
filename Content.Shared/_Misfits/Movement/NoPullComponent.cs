// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_Misfits/Movement/NoPullComponent.cs

namespace Content.Shared._Misfits.Movement;

/// <summary>
/// This entity can't be pulled or dragged by anyone. Intended for heavy robots such as the Sentry Bot.
/// </summary>
[RegisterComponent]
public sealed partial class NoPullComponent : Component;
