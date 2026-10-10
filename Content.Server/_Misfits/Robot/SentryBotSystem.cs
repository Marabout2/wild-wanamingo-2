// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Server/_Misfits/Robot/SentryBotOverheatSystem.cs,
// SentryBotMissileLauncherSystem.cs
// Changed for Wild Wanamingo: merged into one system; missile settings moved onto the component.

using Content.Server.Chat.Systems;
using Content.Shared._Misfits.Robot;
using Content.Shared.Actions;
using Content.Shared.Chat;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Server._Misfits.Robot;

public sealed partial class SentryBotSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedGunSystem _gun = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private ChatSystem _chat = default!;

    private const float UpdateInterval = 0.5f;
    private float _accumulator;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SentryBotChassisComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SentryBotChassisComponent, GunShotEvent>(OnGunShot);
        SubscribeLocalEvent<SentryBotChassisComponent, SentryBotMissileLaunchEvent>(OnMissileLaunch);
    }

    private void OnMapInit(Entity<SentryBotChassisComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent, ref ent.Comp.MissileLaunchActionEntity, ent.Comp.MissileAction);
    }

    // The built-in gun shoots itself dry, then overheats until its ammo refills.
    private void OnGunShot(Entity<SentryBotChassisComponent> ent, ref GunShotEvent args)
    {
        if (ent.Comp.Overheating || !TryComp<BasicEntityAmmoProviderComponent>(ent, out var ammo) || ammo.Count > 0)
            return;

        ent.Comp.Overheating = true;
        ent.Comp.OverheatEndTime = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.OverheatDuration);
        _popup.PopupEntity(Loc.GetString("sentrybot-overheat-warning"), ent, PopupType.LargeCaution);
    }

    private void OnMissileLaunch(Entity<SentryBotChassisComponent> ent, ref SentryBotMissileLaunchEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        if (ent.Comp.IsTargeting)
            return;

        ent.Comp.IsTargeting = true;
        ent.Comp.TargetCoords = args.Target;
        ent.Comp.MissileLaunchTime = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.TargetingDelay);
        Emote(ent, "sentrybot-targeting-emote");

        foreach (var nearby in _lookup.GetEntitiesInRange(args.Target, ent.Comp.WarningRange))
        {
            if (nearby != ent.Owner)
                _popup.PopupEntity(Loc.GetString("sentrybot-missile-lock-warning"), nearby, nearby, PopupType.LargeCaution);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _accumulator += frameTime;
        if (_accumulator < UpdateInterval)
            return;
        _accumulator -= UpdateInterval;

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<SentryBotChassisComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Overheating && now >= comp.OverheatEndTime && TryComp<BasicEntityAmmoProviderComponent>(uid, out var ammo))
            {
                comp.Overheating = false;
                ammo.Count = ammo.Capacity;
                Dirty(uid, ammo);
                _popup.PopupEntity(Loc.GetString("sentrybot-systems-nominal"), uid, PopupType.Medium);
            }

            if (comp.IsTargeting && now >= comp.MissileLaunchTime)
            {
                comp.IsTargeting = false;
                LaunchMissile((uid, comp));
            }
        }
    }

    private void LaunchMissile(Entity<SentryBotChassisComponent> ent)
    {
        Emote(ent, "sentrybot-missile-launched-emote");

        var from = Transform(ent).Coordinates;
        var missile = Spawn(ent.Comp.MissilePrototype, _transform.ToMapCoordinates(from));
        var direction = _transform.ToMapCoordinates(ent.Comp.TargetCoords).Position - _transform.ToMapCoordinates(from).Position;
        _gun.ShootProjectile(missile, direction, _physics.GetMapLinearVelocity(ent), ent, ent, ent.Comp.MissileSpeed);
    }

    private void Emote(EntityUid uid, string locId)
    {
        _chat.TrySendInGameICMessage(uid, Loc.GetString(locId), InGameICChatType.Emote, ChatTransmitRange.Normal,
            ignoreActionBlocker: true);
    }
}
