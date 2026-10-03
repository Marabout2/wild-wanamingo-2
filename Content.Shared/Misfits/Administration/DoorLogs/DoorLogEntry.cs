using Robust.Shared.Serialization;

namespace Content.Shared.Misfits.Administration.DoorLogs;

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
