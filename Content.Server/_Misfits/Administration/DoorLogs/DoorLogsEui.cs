// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Server/_Misfits/Administration/DoorLogs/DoorLogsEui.cs

using Content.Server.EUI;
using Content.Shared.Eui;
using Content.Shared._Misfits.Administration.DoorLogs;

namespace Content.Server._Misfits.Administration.DoorLogs;

/// <summary>
/// Misfits: server side of the door logs admin panel. Read-only, refreshed when doors break.
/// </summary>
public sealed partial class DoorLogsEui : BaseEui
{
    [Dependency] private IEntityManager _entityManager = default!;

    private readonly DoorLogSystem _doorLog;

    public DoorLogsEui()
    {
        IoCManager.InjectDependencies(this);
        _doorLog = _entityManager.System<DoorLogSystem>();
    }

    public override EuiStateBase GetNewState()
    {
        return new DoorLogsEuiState(_doorLog.GetEntries());
    }

    public override void Opened()
    {
        base.Opened();
        _doorLog.RegisterUi(this);
        StateDirty();
    }

    public override void Closed()
    {
        base.Closed();
        _doorLog.UnregisterUi(this);
    }
}
