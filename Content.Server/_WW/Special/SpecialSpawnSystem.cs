using Content.Shared._Misfits.Special;
using Content.Shared._Misfits.SpecialStats;
using Content.Shared.GameTicking;

namespace Content.Server._WW.Special;

/// <summary>
/// Gives a freshly spawned character the SPECIAL stats from their profile.
/// (Misfits did this inside StationSpawningSystem; here it hangs off the spawn event instead.)
/// </summary>
public sealed partial class SpecialSpawnSystem : EntitySystem
{
    [Dependency] private SharedSpecialSystem _special = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        if (!_special.TryApplyProfileBaseValues(args.Mob, args.Profile))
            return;

        // Stat effects that patch other components (health thresholds, HUDs, accents) apply now.
        var ev = new SpecialStatsReadyEvent(args.Mob);
        RaiseLocalEvent(args.Mob, ref ev, true);
    }
}
