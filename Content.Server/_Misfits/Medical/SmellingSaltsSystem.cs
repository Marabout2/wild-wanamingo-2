// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Server/_Misfits/Medical/SmellingSaltsSystem.cs
// Changed for Wild Wanamingo: rewritten as a single-use defibrillator on Wizden's revive rules (rot, Unrevivable,
// dead threshold, return-to-body prompt), instead of Misfits' resuscitation system and SPECIAL do-after scaling.

using Content.Server.EUI;
using Content.Server.Ghost;
using Content.Shared._Misfits.Medical;
using Content.Shared.Atmos.Rotting;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Traits.Assorted;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Server._Misfits.Medical;

/// <summary>
/// Smelling salts: a consumable defibrillator. A few seconds of use heals the patient and, if that brings a dead
/// body back under its death threshold, revives it to critical. No power or shock; the salts are used up.
/// </summary>
public sealed partial class SmellingSaltsSystem : EntitySystem
{
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private EuiManager _eui = default!;
    [Dependency] private ISharedPlayerManager _player = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private MobThresholdSystem _thresholds = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedMindSystem _mind = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedRottingSystem _rotting = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SmellingSaltsComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<SmellingSaltsComponent, SmellingSaltsDoAfterEvent>(OnDoAfter);
    }

    private void OnAfterInteract(Entity<SmellingSaltsComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target is not { } target || !CanUseOn(ent, target, args.User))
            return;

        args.Handled = true;
        _audio.PlayPvs(ent.Comp.UseSound, ent);
        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, ent.Comp.DoAfterDuration,
            new SmellingSaltsDoAfterEvent(), ent, target, ent)
        {
            BlockDuplicate = true,
            BreakOnHandChange = true,
            NeedHand = true,
            BreakOnMove = !ent.Comp.AllowMovement,
        });
    }

    private void OnDoAfter(Entity<SmellingSaltsComponent> ent, ref SmellingSaltsDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Target is not { } target || !CanUseOn(ent, target, args.User))
            return;

        args.Handled = true;
        _damageable.TryChangeDamage(target, ent.Comp.ReviveHeal, true, origin: args.User);

        if (_mobState.IsDead(target) &&
            _thresholds.TryGetThresholdForState(target, MobState.Dead, out var dead) &&
            _damageable.GetTotalDamage(target) < dead)
        {
            _mobState.ChangeMobState(target, MobState.Critical, origin: args.User);
            _popup.PopupEntity(Loc.GetString("smelling-salts-revive-do", ("target", Identity.Entity(target, EntityManager))),
                target, PopupType.Medium);
        }

        // Like a defibrillator: bring the patient's ghost back, or tell the user there's nobody home.
        if (_mind.TryGetMind(target, out var mindId, out var mind) && _player.TryGetSessionById(mind.UserId, out var session))
        {
            if (mind.CurrentEntity != target)
                _eui.OpenEui(new ReturnToBodyEui(mind, _mind, _player), session);
        }
        else if (HasComp<MindContainerComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("smelling-salts-no-mind"), target, args.User);
        }

        QueueDel(ent);
    }

    /// <summary>
    /// Dead patients (or critical ones, if the salts allow it) that aren't rotten or unrevivable.
    /// </summary>
    private bool CanUseOn(Entity<SmellingSaltsComponent> ent, EntityUid target, EntityUid user)
    {
        if (!TryComp<MobStateComponent>(target, out var state))
            return false;

        if (!_mobState.IsDead(target, state) && !(ent.Comp.CanReviveCrit && _mobState.IsCritical(target, state)))
            return false;

        if (_rotting.IsRotten(target))
        {
            _popup.PopupEntity(Loc.GetString("smelling-salts-rotten"), target, user);
            return false;
        }

        if (TryComp<UnrevivableComponent>(target, out var unrevivable))
        {
            _popup.PopupEntity(Loc.GetString(unrevivable.ReasonMessage), target, user);
            return false;
        }

        return true;
    }
}
