// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_Misfits/Movement/NoPullSystem.cs

using Content.Shared.Pulling.Events;

namespace Content.Shared._Misfits.Movement;

/// <summary>
/// Cancels any attempt to pull an entity with <see cref="NoPullComponent"/>, by players or NPCs.
/// </summary>
public sealed partial class NoPullSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NoPullComponent, BeingPulledAttemptEvent>(OnBeingPulledAttempt);
    }

    private void OnBeingPulledAttempt(Entity<NoPullComponent> ent, ref BeingPulledAttemptEvent args)
    {
        args.Cancel();
    }
}
