// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Server/_Misfits/Silicon/SiliconInternalRadioSystem.cs
// Changed for Wild Wanamingo: the assaultron combat-mode block isn't ported (it hooked a Misfits combat mode event;
// assaultrons use Wizden's WieldingBlocker instead).

using Content.Shared._Misfits.Silicon;
using Content.Shared.Mobs.Systems;
using Content.Shared.Radio;
using Content.Shared.Radio.Components;

namespace Content.Server._Misfits.Silicon;

public sealed partial class SiliconInternalRadioSystem : EntitySystem
{
    [Dependency] private MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SiliconInternalRadioComponent, RadioSendAttemptEvent>(OnRadioSendAttempt);
        SubscribeLocalEvent<SiliconInternalRadioComponent, RadioReceiveAttemptEvent>(OnRadioReceiveAttempt);
        SubscribeLocalEvent<SiliconInternalRadioComponent, GetDefaultRadioChannelEvent>(OnGetDefaultRadioChannel);
    }

    private void OnRadioSendAttempt(Entity<SiliconInternalRadioComponent> ent, ref RadioSendAttemptEvent args)
    {
        if (!_mobState.IsAlive(ent))
            args.Cancelled = true;
    }

    private void OnRadioReceiveAttempt(Entity<SiliconInternalRadioComponent> ent, ref RadioReceiveAttemptEvent args)
    {
        if (!_mobState.IsAlive(ent))
            args.Cancelled = true;
    }

    private void OnGetDefaultRadioChannel(EntityUid uid, SiliconInternalRadioComponent component, GetDefaultRadioChannelEvent args)
    {
        if (TryComp<EncryptionKeyHolderComponent>(uid, out var keyHolder))
            args.Channel ??= keyHolder.DefaultChannel;
    }
}
