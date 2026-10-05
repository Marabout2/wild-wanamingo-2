// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_Misfits/Administration/DoorLogs/DoorLogsEuiState.cs

using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared._Misfits.Administration.DoorLogs;

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
