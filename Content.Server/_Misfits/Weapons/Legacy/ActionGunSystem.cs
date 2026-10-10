// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/Weapons/Ranged/Systems/ActionGunSystem.cs
// and Content.Server/_Misfits/Robot/AssaultronBeamChargeSystem.cs (Misfits' copy of a Space Station 14 file plus its
// assaultron charge-up). Changed for Wild Wanamingo: one server system; the action's cooldown shows the charge-up.

using Content.Server.Chat.Systems;
using Content.Shared._Misfits.Robot;
using Content.Shared._Misfits.Weapons.Legacy;
using Content.Shared.Actions;
using Content.Shared.Chat;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Timing;

namespace Content.Server._Misfits.Weapons.Legacy;

public sealed partial class ActionGunSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedGunSystem _gun = default!;
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActionGunComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ActionGunComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<ActionGunComponent, ActionGunShootEvent>(OnShoot);
    }

    private void OnMapInit(Entity<ActionGunComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent, ref ent.Comp.ActionEntity, ent.Comp.Action);
        ent.Comp.Gun = Spawn(ent.Comp.GunProto);
    }

    private void OnShutdown(Entity<ActionGunComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.Gun is { } gun)
            QueueDel(gun);
    }

    private void OnShoot(Entity<ActionGunComponent> ent, ref ActionGunShootEvent args)
    {
        args.Handled = true;

        if (ent.Comp.Gun is not { } gunUid || !TryComp<GunComponent>(gunUid, out var gun))
            return;

        if (!TryComp<AssaultronBeamChargeComponent>(gunUid, out var charge))
        {
            _gun.AttemptShoot(args.Performer, (gunUid, gun), args.Target);
            return;
        }

        var now = _timing.CurTime;
        var action = args.Action.Owner;

        // Cooling down after the last shot.
        if (!charge.IsCharging && now < charge.CooldownEndTime)
        {
            _actions.SetCooldown(action, now, charge.CooldownEndTime);
            return;
        }

        // First use: start charging; the action's cooldown shows when it's ready.
        if (!charge.IsCharging)
        {
            charge.IsCharging = true;
            charge.ChargeEndTime = now + TimeSpan.FromSeconds(charge.ChargeDuration);
            _actions.SetCooldown(action, now, charge.ChargeEndTime);
            Emote(args.Performer, charge.ChargeEmoteLocale, ref charge.NextChargeEmoteTime, charge.EmoteCooldown);
            return;
        }

        if (now < charge.ChargeEndTime)
        {
            _actions.SetCooldown(action, now, charge.ChargeEndTime);
            return;
        }

        // Charged: fire, then cool down.
        charge.IsCharging = false;
        if (!_gun.AttemptShoot(args.Performer, (gunUid, gun), args.Target))
            return;

        charge.CooldownEndTime = now + TimeSpan.FromSeconds(charge.CooldownDuration);
        _actions.SetCooldown(action, now, charge.CooldownEndTime);
        Emote(args.Performer, charge.FireEmoteLocale, ref charge.NextFireEmoteTime, charge.EmoteCooldown);
    }

    private void Emote(EntityUid user, LocId message, ref TimeSpan nextAllowed, float cooldown)
    {
        var now = _timing.CurTime;
        if (now < nextAllowed)
            return;

        nextAllowed = now + TimeSpan.FromSeconds(cooldown);
        _chat.TrySendInGameICMessage(user, Loc.GetString(message), InGameICChatType.Emote, ChatTransmitRange.Normal,
            ignoreActionBlocker: true);
    }
}
