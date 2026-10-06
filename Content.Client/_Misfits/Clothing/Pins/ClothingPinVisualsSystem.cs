// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Client/_Misfits/Clothing/Pins/ClothingPinVisualsSystem.cs
// Changed for Wild Wanamingo: pin layers come from the clothing system (ClothingVisuals is access-restricted).

using Content.Client.Clothing;
using Content.Shared._Misfits.Clothing.Pins;
using Content.Shared.Clothing;
using Content.Shared.Clothing.Components;
using Content.Shared.Item;
using Robust.Client.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Serialization.Manager;

namespace Content.Client._Misfits.Clothing.Pins;

/// <summary>
/// Adds attached pin clothing visuals to the worn clothing item.
/// </summary>
public sealed partial class ClothingPinVisualsSystem : EntitySystem
{
    [Dependency] private SharedContainerSystem _containers = default!;
    [Dependency] private SharedItemSystem _item = default!;
    [Dependency] private ISerializationManager _serialization = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ClothingPinHolderComponent, GetEquipmentVisualsEvent>(OnGetEquipmentVisuals, after: [typeof(ClientClothingSystem)]);
        SubscribeLocalEvent<ClothingPinHolderComponent, EntInsertedIntoContainerMessage>(OnContainerChanged);
        SubscribeLocalEvent<ClothingPinHolderComponent, EntRemovedFromContainerMessage>(OnContainerChanged);
        SubscribeLocalEvent<ClothingPinHolderComponent, AfterAutoHandleStateEvent>(OnAfterState);
    }

    private void OnGetEquipmentVisuals(Entity<ClothingPinHolderComponent> ent, ref GetEquipmentVisualsEvent args)
    {
        if (!_containers.TryGetContainer(ent, ent.Comp.ContainerId, out var container))
            return;

        var index = 0;
        foreach (var pin in container.ContainedEntities)
        {
            if (!TryComp<ClothingComponent>(pin, out var clothing))
                continue;

            // WW: Wizden restricts ClothingVisuals, so ask the clothing system for the pin's neck layers.
            var pinVisuals = new GetEquipmentVisualsEvent(args.Equipee, "neck");
            RaiseLocalEvent(pin, pinVisuals);

            foreach (var (_, layer) in pinVisuals.Layers)
            {
                var attachedLayer = _serialization.CreateCopy(layer, notNullableOverride: true);

                // Explicitly use the pin RSI. Otherwise the layer falls back to the
                // uniform/armor RSI and missing pin states render as error textures.
                if (string.IsNullOrWhiteSpace(attachedLayer.RsiPath))
                {
                    attachedLayer.RsiPath = GetPinRsiPath(pin, clothing);

                    if (attachedLayer.RsiPath == null)
                        continue;
                }

                args.Layers.Add(($"misfits-clothing-pin-{pin.Id}-{index}", attachedLayer));
                index++;
            }
        }
    }

    private string? GetPinRsiPath(EntityUid pin, ClothingComponent clothing)
    {
        if (!string.IsNullOrWhiteSpace(clothing.RsiPath))
            return NormalizeRelativeRsiPath(clothing.RsiPath);

        return TryComp<SpriteComponent>(pin, out var sprite)
            ? sprite.BaseRSI?.Path.CanonPath
            : null;
    }

    private static string NormalizeRelativeRsiPath(string path)
    {
        path = path.TrimStart('/');

        const string textureRoot = "Textures/";
        return path.StartsWith(textureRoot, StringComparison.Ordinal)
            ? path[textureRoot.Length..]
            : path;
    }

    private void OnContainerChanged(Entity<ClothingPinHolderComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        _item.VisualsChanged(ent.Owner);
    }

    private void OnContainerChanged(Entity<ClothingPinHolderComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        _item.VisualsChanged(ent.Owner);
    }

    private void OnAfterState(Entity<ClothingPinHolderComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        _item.VisualsChanged(ent.Owner);
    }
}
