// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_Misfits/Administration/DoorLogs/DoorLogEntry.cs

using Robust.Shared.Serialization;

namespace Content.Shared._Misfits.Administration.DoorLogs;

/// <summary>
/// Misfits: one door destruction, for the door logs admin panel.
/// </summary>
[Serializable, NetSerializable]
public sealed class DoorLogEntry
{
    public string DoorPrototype;
    public string DestroyedBy;
    public TimeSpan Time;

    public DoorLogEntry(string doorPrototype, string destroyedBy, TimeSpan time)
    {
        DoorPrototype = doorPrototype;
        DestroyedBy = destroyedBy;
        Time = time;
    }
}
