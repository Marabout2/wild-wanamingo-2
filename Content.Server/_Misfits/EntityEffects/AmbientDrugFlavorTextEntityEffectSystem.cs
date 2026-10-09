// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Server/_Misfits/EntityEffects/Effects/DrugAmbience/AmbientDrugFlavorText.cs
// Changed for Wild Wanamingo: rewritten as an EntityEffectSystem.

using Content.Server._Misfits.DrugEffects;
using Content.Server.Chat.Managers;
using Content.Shared._Misfits.EntityEffects;
using Content.Shared.Chat;
using Content.Shared.EntityEffects;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Misfits.EntityEffects;

public sealed partial class AmbientDrugFlavorTextEntityEffectSystem : EntityEffectSystem<ActorComponent, AmbientDrugFlavorText>
{
    [Dependency] private IChatManager _chat = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IRobustRandom _random = default!;

    protected override void Effect(Entity<ActorComponent> entity, ref EntityEffectEvent<AmbientDrugFlavorText> args)
    {
        var effect = args.Effect;
        if (effect.MessageKeys.Count == 0)
            return;

        var cooldowns = EnsureComp<DrugAmbienceCooldownComponent>(entity);
        var id = string.IsNullOrWhiteSpace(effect.CooldownId) ? string.Join("|", effect.MessageKeys) : effect.CooldownId;
        if (cooldowns.NextAllowedById.TryGetValue(id, out var next) && next > _timing.CurTime)
            return;

        cooldowns.NextAllowedById[id] = _timing.CurTime + TimeSpan.FromSeconds(effect.Cooldown);

        var text = Loc.GetString(_random.Pick(effect.MessageKeys));
        _chat.ChatMessageToOne(ChatChannel.Local, text, text, EntityUid.Invalid, false, entity.Comp.PlayerSession.Channel);
    }
}
