// Ported from Nuclear 14 (https://github.com/Vault-Overseers/nuclear-14) via Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_N14/Radiation/Systems/RadiationHealingSystem.cs
// Changed for Wild Wanamingo: moved from _N14 to _Nuclear14; reads damage through DamageableSystem (access rules).

using Content.Shared.Movement.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared._Nuclear14.Radiation.Components;
using Content.Shared.FixedPoint;

namespace Content.Shared._Nuclear14.Radiation;

public sealed partial class RadiationHealingSystem : EntitySystem
{
    [Dependency] private DamageableSystem _damageable = default!;

    private static readonly string[] HealableTypes = { "Blunt", "Slash", "Piercing", "Heat" };

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RadiationHealingComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMove);
    }

    private void OnRefreshMove(EntityUid uid, RadiationHealingComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        if (component.CurrentExposure <= 0f)
            return;

        if (!TryComp(uid, out DamageableComponent? damage) || _damageable.GetTotalDamage((uid, damage)) >= 90f)
            return;

        var healable = GetHealableDamage(_damageable.GetAllDamage((uid, damage)));
        if (healable <= 0f)
            return;

        var mod = Math.Clamp(1f - component.CurrentExposure * component.SlowFactor, 0.5f, 1f);
        args.ModifySpeed(mod);
    }

    private static float GetHealableDamage(DamageSpecifier damage)
    {
        float amount = 0f;
        foreach (var type in HealableTypes)
        {
            if (damage.DamageDict.TryGetValue(type, out var val) && val > FixedPoint2.Zero)
                amount += val.Float();
        }

        return amount;
    }
}
