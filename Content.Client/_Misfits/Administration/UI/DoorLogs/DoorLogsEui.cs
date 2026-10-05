// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Client/_Misfits/Administration/UI/DoorLogs/DoorLogsEui.cs

using Content.Client.Eui;
using Content.Shared.Eui;
using Content.Shared._Misfits.Administration.DoorLogs;

namespace Content.Client._Misfits.Administration.UI.DoorLogs;

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
