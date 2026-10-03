using Content.Client.Eui;
using Content.Shared.Eui;
using Content.Shared.Misfits.Administration.DoorLogs;

namespace Content.Client.Misfits.Administration.UI.DoorLogs;

/// <summary>
/// Misfits: client side of the door logs admin panel.
/// </summary>
public sealed class DoorLogsEui : BaseEui
{
    private readonly DoorLogsWindow _window;

    public DoorLogsEui()
    {
        _window = new DoorLogsWindow();
        _window.OnClose += () => SendMessage(new CloseEuiMessage());
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is DoorLogsEuiState cast)
            _window.Populate(cast.Entries);
    }

    public override void Opened()
    {
        base.Opened();
        _window.OpenCentered();
    }

    public override void Closed()
    {
        base.Closed();
        _window.Close();
    }
}
