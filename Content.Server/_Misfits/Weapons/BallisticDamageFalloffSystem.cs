// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Server/_Misfits/Weapons/BallisticDamageFalloffSystem.cs
// Also applies GunDamageModifier and GunDamageFalloffOverride, which Misfits did inside Wizden's gun system;
// here they hook AmmoShotEvent instead.

using Content.Shared._Misfits.Weapons;
using Content.Shared._Misfits.Weapons.Components;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Map;

namespace Content.Server._Misfits.Weapons;

/// <summary>
/// Scales ballistic projectile damage by distance travelled, and applies gun-side damage bonuses and falloff
/// overrides to the projectiles a gun fires.
/// </summary>
public sealed partial class BallisticDamageFalloffSystem : EntitySystem
{
    [Dependency] private SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BallisticDamageFalloffComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<BallisticDamageFalloffComponent, ProjectileHitEvent>(OnProjectileHit);
        SubscribeLocalEvent<GunDamageModifierComponent, AmmoShotEvent>(OnModifierShot);
        SubscribeLocalEvent<GunDamageFalloffOverrideComponent, AmmoShotEvent>(OnOverrideShot);
    }

    private void OnMapInit(Entity<BallisticDamageFalloffComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.SpawnPosition = _transform.GetMapCoordinates(ent);
    }

    private void OnModifierShot(Entity<GunDamageModifierComponent> ent, ref AmmoShotEvent args)
    {
        foreach (var projectile in args.FiredProjectiles)
        {
            if (TryComp<ProjectileComponent>(projectile, out var proj))
                proj.Damage += ent.Comp.Damage;
        }
    }

    private void OnOverrideShot(Entity<GunDamageFalloffOverrideComponent> ent, ref AmmoShotEvent args)
    {
        foreach (var projectile in args.FiredProjectiles)
        {
            var falloff = EnsureComp<BallisticDamageFalloffComponent>(projectile);
            falloff.FalloffStartTiles = ent.Comp.FalloffStartTiles;
            falloff.MaxFalloffTiles = ent.Comp.MaxFalloffTiles;
            falloff.MinDamageMultiplier = ent.Comp.MinDamageMultiplier;
            if (falloff.SpawnPosition == MapCoordinates.Nullspace)
                falloff.SpawnPosition = _transform.GetMapCoordinates(projectile);
        }
    }

    private void OnProjectileHit(Entity<BallisticDamageFalloffComponent> ent, ref ProjectileHitEvent args)
    {
        var comp = ent.Comp;
        if (comp.SpawnPosition == MapCoordinates.Nullspace)
            return;

        var currentPos = _transform.GetMapCoordinates(ent);
        if (currentPos.MapId != comp.SpawnPosition.MapId)
            return;

        var distance = (currentPos.Position - comp.SpawnPosition.Position).Length();
        if (distance <= comp.FalloffStartTiles)
            return;

        var range = comp.MaxFalloffTiles - comp.FalloffStartTiles;
        if (range <= 0f)
            return;

        // 1.0 at FalloffStartTiles down to MinDamageMultiplier at MaxFalloffTiles.
        var fraction = Math.Clamp((distance - comp.FalloffStartTiles) / range, 0f, 1f);
        args.Damage *= 1f - fraction * (1f - comp.MinDamageMultiplier);
    }
}
