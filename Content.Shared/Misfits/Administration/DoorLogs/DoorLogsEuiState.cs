using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared.Misfits.Administration.DoorLogs;

/// <summary>
/// Misfits: all logged door destructions, newest first.
/// </summary>
[Serializable, NetSerializable]
public sealed class DoorLogsEuiState : EuiStateBase
{
    public List<DoorLogEntry> Entries;

    public DoorLogsEuiState(List<DoorLogEntry> entries)
    {
        Entries = entries;
    }
}
