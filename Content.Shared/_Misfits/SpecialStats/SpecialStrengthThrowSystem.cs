// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_Misfits/SpecialStats/SpecialStrengthThrowSystem.cs

using Content.Shared._Misfits.Special;
using Content.Shared._Misfits.Special.Components;
using Content.Shared.Throwing;

namespace Content.Shared._Misfits.SpecialStats;

public sealed partial class SpecialStrengthThrowSystem : EntitySystem
{
    [Dependency] private SharedSpecialSystem _special = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpecialComponent, BeforeThrowEvent>(OnBeforeThrow);
    }

    private void OnBeforeThrow(Entity<SpecialComponent> ent, ref BeforeThrowEvent args)
    {
        args.ThrowSpeed *= _special.GetStrengthThrowSpeedMultiplier(ent.Owner, ent.Comp);
    }
}
