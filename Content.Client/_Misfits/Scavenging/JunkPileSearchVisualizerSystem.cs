// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Client/_Misfits/Scavenging/JunkPileSearchVisualizerSystem.cs

using System.Numerics;
using Content.Shared._Misfits.Scavenging;
using Robust.Client.GameObjects;

namespace Content.Client._Misfits.Scavenging;

/// <summary>
/// Makes searched junk piles visibly smaller until their shared loot cooldown has elapsed.
/// </summary>
public sealed partial class JunkPileSearchVisualizerSystem : VisualizerSystem<JunkPileSearchableComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, JunkPileSearchableComponent component,
        ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null ||
            !AppearanceSystem.TryGetData(uid, JunkPileVisuals.Depleted, out bool depleted, args.Component))
            return;

        SpriteSystem.SetScale((uid, args.Sprite), depleted ? new Vector2(0.45f) : new Vector2(0.8f));
    }
}
