// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Server/_Misfits/Administration/Commands/DoorLogsCommand.cs

using Content.Server.Administration;
using Content.Server.EUI;
using Content.Server._Misfits.Administration.DoorLogs;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._Misfits.Administration.Commands;

/// <summary>
/// Misfits: opens the door logs panel (which doors were destroyed this round, and by whom).
/// </summary>
[AdminCommand(AdminFlags.Admin)]
public sealed partial class DoorLogsCommand : IConsoleCommand
{
    [Dependency] private EuiManager _eui = default!;

    public string Command => "doorlogs";
    public string Description => "Opens the door destruction log for this round.";
    public string Help => "doorlogs";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player is not { } player)
        {
            shell.WriteError("This command can only be run by a player.");
            return;
        }

        _eui.OpenEui(new DoorLogsEui(), player);
    }
}
