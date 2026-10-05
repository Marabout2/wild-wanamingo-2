// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Server/_Misfits/Sound/AggroSoundSystem.cs

using Content.Server.NPC.Components;
using Content.Shared._Misfits.Sound;
using Content.Shared.Mobs.Systems;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Server.Audio;
using Robust.Shared.Collections;
using Robust.Shared.Random;

namespace Content.Server._Misfits.Sound;

/// <summary>
/// Plays the <see cref="AggroSoundComponent"/> sound when an NPC starts fighting or attacks (melee or ranged),
/// with a cooldown to prevent spam.
/// </summary>
public sealed partial class AggroSoundSystem : EntitySystem
{
    [Dependency] private AudioSystem _audio = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private IRobustRandom _random = default!;

    // Only entities with an active cooldown are tracked, so Update is O(active) rather than O(all NPCs).
    private readonly HashSet<EntityUid> _activeCooldowns = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AggroSoundComponent, MeleeAttackEvent>(OnMeleeAttack);
        SubscribeLocalEvent<AggroSoundComponent, GunShotEvent>(OnGunShot);
        SubscribeLocalEvent<NPCMeleeCombatComponent, ComponentInit>(OnMeleeCombatStartup);
        SubscribeLocalEvent<NPCRangedCombatComponent, ComponentInit>(OnRangedCombatStartup);
        SubscribeLocalEvent<AggroSoundComponent, ComponentShutdown>(OnAggroShutdown);
    }

    private void OnAggroShutdown(Entity<AggroSoundComponent> ent, ref ComponentShutdown args)
    {
        _activeCooldowns.Remove(ent);
    }

    private void OnMeleeAttack(Entity<AggroSoundComponent> ent, ref MeleeAttackEvent args)
    {
        TryPlayAggro(ent);
    }

    // Fires on the gun; mobs that are their own gun get it directly.
    private void OnGunShot(Entity<AggroSoundComponent> ent, ref GunShotEvent args)
    {
        TryPlayAggro(ent);
    }

    // NPC combat starting counts as aggro, so ranged mobs call out before their first shot.
    private void OnMeleeCombatStartup(Entity<NPCMeleeCombatComponent> ent, ref ComponentInit args)
    {
        if (TryComp<AggroSoundComponent>(ent, out var aggro))
            TryPlayAggro((ent, aggro));
    }

    private void OnRangedCombatStartup(Entity<NPCRangedCombatComponent> ent, ref ComponentInit args)
    {
        if (TryComp<AggroSoundComponent>(ent, out var aggro))
            TryPlayAggro((ent, aggro));
    }

    private void TryPlayAggro(Entity<AggroSoundComponent> ent)
    {
        if (_mobState.IsDead(ent) || ent.Comp.CooldownRemaining > 0f)
            return;

        _audio.PlayPvs(ent.Comp.Sound, ent);
        ent.Comp.CooldownRemaining = _random.NextFloat(ent.Comp.CooldownMin, ent.Comp.CooldownMax);
        _activeCooldowns.Add(ent);
        Dirty(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var toRemove = new ValueList<EntityUid>();
        foreach (var uid in _activeCooldowns)
        {
            if (!TryComp<AggroSoundComponent>(uid, out var aggro))
            {
                toRemove.Add(uid);
                continue;
            }

            aggro.CooldownRemaining -= frameTime;
            if (aggro.CooldownRemaining > 0f)
                continue;

            aggro.CooldownRemaining = 0f;
            toRemove.Add(uid);
            Dirty(uid, aggro);
        }

        foreach (var uid in toRemove)
        {
            _activeCooldowns.Remove(uid);
        }
    }
}
