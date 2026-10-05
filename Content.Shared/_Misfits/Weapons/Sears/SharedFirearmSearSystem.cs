// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_Misfits/Weapons/Sears/SharedFirearmSearSystem.cs
// Changed for Wild Wanamingo: popups are localized.

using Content.Shared.Containers.ItemSlots;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Network;

namespace Content.Shared._Misfits.Weapons.Sears;

/// <summary>
/// Installs burst and automatic sears into compatible firearms, used up on install.
/// </summary>
public sealed partial class SharedFirearmSearSystem : EntitySystem
{
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private ItemSlotsSystem _itemSlots = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private SharedGunSystem _gun = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FirearmSearComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<FirearmSearComponent, InstallFirearmSearDoAfterEvent>(OnInstallFinished);
        SubscribeLocalEvent<InstalledFirearmSearComponent, ComponentInit>(OnInstalledInit);
    }

    private void OnAfterInteract(Entity<FirearmSearComponent> sear, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target is not { } target ||
            !TryComp<GunComponent>(target, out var gun))
        {
            return;
        }

        args.Handled = true;
        if (!CanInstall(target, sear.Comp.Mode, gun, out var reason))
        {
            _popup.PopupEntity(Loc.GetString(reason), target, args.User);
            return;
        }

        _popup.PopupEntity(Loc.GetString("firearm-sear-install-start"), target, args.User);
        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager,
            args.User,
            sear.Comp.InstallTime,
            new InstallFirearmSearDoAfterEvent(),
            sear,
            target: target,
            used: sear)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            CancelDuplicate = true,
            NeedHand = true,
        });
    }

    private void OnInstallFinished(Entity<FirearmSearComponent> sear, ref InstallFirearmSearDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target is not { } target ||
            !TryComp<GunComponent>(target, out var gun))
        {
            return;
        }

        args.Handled = true;
        if (!CanInstall(target, sear.Comp.Mode, gun, out var reason))
        {
            _popup.PopupEntity(Loc.GetString(reason), target, args.User);
            return;
        }

        if (!_net.IsServer)
            return;

        AddComp(target, new InstalledFirearmSearComponent { Mode = sear.Comp.Mode });
        _gun.AddFireMode(target, sear.Comp.Mode, gun);
        QueueDel(sear);
        _popup.PopupEntity(Loc.GetString("firearm-sear-install-done"), target, args.User);
    }

    private void OnInstalledInit(Entity<InstalledFirearmSearComponent> ent, ref ComponentInit args)
    {
        if (TryComp<GunComponent>(ent, out var gun))
            _gun.AddFireMode(ent, ent.Comp.Mode, gun);
    }

    private bool CanInstall(EntityUid target, SelectiveFire mode, GunComponent gun, out string reason)
    {
        reason = string.Empty;
        if (!TryComp<FirearmSearCompatibleComponent>(target, out var compatible) ||
            mode == SelectiveFire.Burst && !compatible.AllowBurst ||
            mode == SelectiveFire.FullAuto && !compatible.AllowFullAuto)
        {
            reason = "firearm-sear-incompatible";
            return false;
        }

        if (HasComp<InstalledFirearmSearComponent>(target))
        {
            reason = "firearm-sear-already-installed";
            return false;
        }

        if ((gun.AvailableModes & mode) != 0)
        {
            reason = "firearm-sear-mode-present";
            return false;
        }

        if (SlotHasItem(target, "gun_magazine") || SlotHasItem(target, "gun_chamber"))
        {
            reason = "firearm-sear-unload-first";
            return false;
        }

        return true;
    }

    private bool SlotHasItem(EntityUid uid, string slotId)
    {
        return _itemSlots.TryGetSlot(uid, slotId, out var slot) && slot.HasItem;
    }
}
