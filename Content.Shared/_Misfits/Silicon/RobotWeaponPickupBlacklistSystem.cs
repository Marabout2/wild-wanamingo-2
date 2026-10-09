// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_Misfits/Silicon/SharedRobotWeaponPickupBlacklistSystem.cs

using Content.Shared.Item;
using Content.Shared.Whitelist;

namespace Content.Shared._Misfits.Silicon;

public sealed partial class RobotWeaponPickupBlacklistSystem : EntitySystem
{
    [Dependency] private EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RobotWeaponPickupBlacklistComponent, PickupAttemptEvent>(OnPickupAttempt);
    }

    private void OnPickupAttempt(EntityUid uid, RobotWeaponPickupBlacklistComponent component, PickupAttemptEvent args)
    {
        if (_whitelist.IsWhitelistPass(component.Blacklist, args.Item))
            args.Cancel();
    }
}
