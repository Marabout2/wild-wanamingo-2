// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Server/_Misfits/Sound/IdleSoundSystem.cs

using Content.Shared.Audio;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Sound;
using Content.Shared.Sound.Components;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Collections;

namespace Content.Server._Misfits.Sound;

/// <summary>
/// Temporarily disables <see cref="SpamEmitSoundComponent"/> when an entity with
/// <see cref="IdleSoundComponent"/> attacks, then re-enables it after a cooldown.
/// Also disables idle sounds while the entity is not alive.
/// </summary>
public sealed partial class IdleSoundSystem : EntitySystem
{
    [Dependency] private SharedAmbientSoundSystem _ambient = default!;
    [Dependency] private SharedEmitSoundSystem _emitSound = default!;
    [Dependency] private MobStateSystem _mobState = default!;

    // Only temporarily suppressed entities are tracked, so Update is O(suppressed) rather than O(all NPCs).
    private readonly HashSet<EntityUid> _suppressedEntities = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<IdleSoundComponent, MeleeAttackEvent>(OnMeleeAttack);
        SubscribeLocalEvent<IdleSoundComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<IdleSoundComponent, ComponentShutdown>(OnIdleShutdown);
    }

    private void OnIdleShutdown(Entity<IdleSoundComponent> ent, ref ComponentShutdown args)
    {
        _suppressedEntities.Remove(ent);
    }

    private void OnMeleeAttack(Entity<IdleSoundComponent> ent, ref MeleeAttackEvent args)
    {
        Suppress(ent);
    }

    private void OnMobStateChanged(Entity<IdleSoundComponent> ent, ref MobStateChangedEvent args)
    {
        var alive = args.NewMobState == MobState.Alive;
        ent.Comp.Suppressed = !alive;
        ent.Comp.CooldownRemaining = 0f;
        _suppressedEntities.Remove(ent);
        _emitSound.SetEnabled((ent.Owner, (SpamEmitSoundComponent?) null), alive);
        _ambient.SetAmbience(ent, alive);
    }

    private void Suppress(Entity<IdleSoundComponent> ent)
    {
        ent.Comp.CooldownRemaining = ent.Comp.CooldownDuration;

        if (ent.Comp.Suppressed)
            return;

        ent.Comp.Suppressed = true;
        _suppressedEntities.Add(ent);
        _emitSound.SetEnabled((ent.Owner, (SpamEmitSoundComponent?) null), false);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var toRemove = new ValueList<EntityUid>();
        foreach (var uid in _suppressedEntities)
        {
            if (!TryComp<IdleSoundComponent>(uid, out var idle))
            {
                toRemove.Add(uid);
                continue;
            }

            idle.CooldownRemaining -= frameTime;
            if (idle.CooldownRemaining > 0f)
                continue;

            toRemove.Add(uid);

            // Dead mobs stay silent.
            if (!_mobState.IsAlive(uid))
                continue;

            idle.Suppressed = false;
            _emitSound.SetEnabled((uid, (SpamEmitSoundComponent?) null), true);
        }

        foreach (var uid in toRemove)
        {
            _suppressedEntities.Remove(uid);
        }
    }
}
