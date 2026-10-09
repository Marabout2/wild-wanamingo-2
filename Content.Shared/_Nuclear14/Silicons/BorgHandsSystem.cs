// Ported from Nuclear 14 (https://github.com/Vault-Overseers/nuclear-14) via Misfits (https://github.com/Misfit-Sanctuary/nuclear-14):
// Content.Shared/_Nuclear14/Silicons/BorgHandsSystem.cs

using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;

namespace Content.Shared._Nuclear14.Silicons;

public sealed partial class BorgHandsSystem : EntitySystem
{
    [Dependency] private SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BorgHandsComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<BorgHandsComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp<HandsComponent>(ent, out var hands))
            return;

        for (var i = 0; i < ent.Comp.HandCount; i++)
        {
            _hands.AddHand((ent, hands), $"borg-hand-{i}", HandLocation.Middle);
        }
    }
}
