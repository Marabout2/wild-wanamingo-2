// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/Nyanotrasen/NPC/Components/Faction/ClothingAddFactionComponent.cs
// (Misfits' copy of a Nyanotrasen file.)

using Content.Shared.NPC.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._Misfits.NPC.Legacy;

/// <summary>
/// Allows clothing to add a faction to you when you wear it (faction IDs, dogtags...).
/// </summary>
[RegisterComponent]
public sealed partial class ClothingAddFactionComponent : Component
{
    public bool IsActive;

    /// <summary>
    /// Faction added while worn.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<NpcFactionPrototype> Faction;
}
