// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Server/_Misfits/Scavenging/JunkPileSearchSystem.cs
// Changed for Wild Wanamingo: loot comes from the component's own list; no Junker Finder talent (talents aren't ported).

using Content.Server._Misfits.SpecialStats;
using Content.Shared._Misfits.Scavenging;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Storage;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Misfits.Scavenging;

/// <summary>
/// Gives mapped junk piles a deliberate, shared search interaction and a cooldown that begins on a successful search.
/// </summary>
public sealed partial class JunkPileSearchSystem : EntitySystem
{
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SpecialLuckSystem _luck = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<JunkPileSearchableComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<JunkPileSearchableComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<JunkPileSearchableComponent, JunkPileSearchDoAfterEvent>(OnSearchComplete);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<JunkPileSearchableComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (component.CooldownEnd == TimeSpan.Zero || component.CooldownEnd > _timing.CurTime)
                continue;

            component.CooldownEnd = TimeSpan.Zero;
            UpdateAppearance((uid, component), false);
        }
    }

    private void OnStartup(Entity<JunkPileSearchableComponent> ent, ref ComponentStartup args)
    {
        UpdateAppearance(ent, false);
    }

    private void OnInteractHand(Entity<JunkPileSearchableComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (ent.Comp.CooldownEnd > _timing.CurTime)
        {
            var remaining = (int) Math.Ceiling((ent.Comp.CooldownEnd - _timing.CurTime).TotalMinutes);
            _popup.PopupEntity(Loc.GetString("junk-pile-search-cooldown", ("minutes", remaining)), ent, args.User,
                PopupType.SmallCaution);
            return;
        }

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, ent.Comp.SearchDuration,
            new JunkPileSearchDoAfterEvent(), ent, ent)
        {
            BlockDuplicate = true,
            BreakOnDamage = true,
            BreakOnMove = true,
            DistanceThreshold = 2f,
        });
    }

    private void OnSearchComplete(Entity<JunkPileSearchableComponent> ent, ref JunkPileSearchDoAfterEvent args)
    {
        if (args.Cancelled || ent.Comp.CooldownEnd > _timing.CurTime || ent.Comp.Contents.Count == 0)
            return;

        ent.Comp.CooldownEnd = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.CooldownSeconds);
        UpdateAppearance(ent, true);

        var coordinates = Transform(ent).Coordinates;
        foreach (var prototype in EntitySpawnCollection.GetSpawns(ent.Comp.Contents, _random))
        {
            Spawn(prototype, coordinates);
        }

        if (TryComp<LuckJunkBonusComponent>(ent, out var luck))
            _luck.TryGrantJunkBonus((ent, luck), args.User);

        _popup.PopupEntity(Loc.GetString("junk-pile-search-complete"), ent, args.User);
    }

    private void UpdateAppearance(Entity<JunkPileSearchableComponent> ent, bool depleted)
    {
        _appearance.SetData(ent, JunkPileVisuals.Depleted, depleted);
    }
}
