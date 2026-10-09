// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Server/_Misfits/Silicon/RobotSelfRepairAnnouncerSystem.cs

using Content.Server.Chat.Systems;
using Content.Shared._Misfits.Silicon;
using Content.Shared.Chat;
using Content.Shared.Damage.Systems;
using Robust.Shared.Timing;

namespace Content.Server._Misfits.Silicon;

public sealed partial class RobotSelfRepairAnnouncerSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RobotSelfRepairAnnouncerComponent, DamageChangedEvent>(OnDamageChanged);
    }

    private void OnDamageChanged(Entity<RobotSelfRepairAnnouncerComponent> ent, ref DamageChangedEvent args)
    {
        var comp = ent.Comp;
        var curTime = _timing.CurTime;
        var total = _damageable.GetTotalDamage(ent.Owner);

        if (args.DamageIncreased && args.DamageDelta != null)
        {
            if (args.DamageDelta.GetTotal() < comp.DamageDeltaThreshold
                || total < comp.MinTotalDamageForHullWarning
                || curTime < comp.NextHullWarningTime)
                return;

            comp.NextHullWarningTime = curTime + comp.HullWarningCooldown;
            Announce(ent, "robot-self-repair-hull-warning");
        }
        else if (!args.DamageIncreased)
        {
            if (total < comp.MinDamageForRepairPopup || curTime < comp.NextRepairAnnounceTime)
                return;

            comp.NextRepairAnnounceTime = curTime + comp.RepairAnnounceCooldown;
            Announce(ent, "robot-self-repair-initiating");
        }
    }

    // Spoken as an emote so people nearby hear it from the robot.
    private void Announce(EntityUid uid, string locId)
    {
        _chat.TrySendInGameICMessage(uid, Loc.GetString(locId), InGameICChatType.Emote, ChatTransmitRange.Normal,
            ignoreActionBlocker: true);
    }
}
