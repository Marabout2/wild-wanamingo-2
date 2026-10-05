// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_Misfits/Wielding/SharedGrantActionOnWieldSystem.cs
// Changed for Wild Wanamingo: uses Wizden's ItemWieldedEvent, which now carries the user, instead of Misfits' own
// wield event (which needed an edit to Wizden's WieldableSystem).

using Content.Shared.Actions;
using Content.Shared.Wieldable;

namespace Content.Shared._Misfits.Wielding;

/// <summary>
/// Grants actions while an item with <see cref="GrantActionOnWieldComponent"/> is wielded, and removes them on unwield.
/// </summary>
/// <remarks>
/// Swapping between two weapons that grant the same action can skip its cooldown.
/// </remarks>
public sealed partial class SharedGrantActionOnWieldSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GrantActionOnWieldComponent, ItemWieldedEvent>(OnWield);
        SubscribeLocalEvent<GrantActionOnWieldComponent, ItemUnwieldedEvent>(OnUnwield);
    }

    private void OnUnwield(Entity<GrantActionOnWieldComponent> ent, ref ItemUnwieldedEvent args)
    {
        foreach (var action in ent.Comp.ActionIds)
        {
            _actions.RemoveAction(args.User, action);
        }
    }

    private void OnWield(Entity<GrantActionOnWieldComponent> ent, ref ItemWieldedEvent args)
    {
        if (ent.Comp.ActionIds.Count > 0)
        {
            foreach (var actionId in ent.Comp.ActionIds)
            {
                _actions.AddActionDirect(args.User, actionId);
            }

            return;
        }

        foreach (var action in ent.Comp.Actions)
        {
            if (_actions.AddAction(args.User, action) is { } actionId)
                ent.Comp.ActionIds.Add(actionId);
        }
    }
}
