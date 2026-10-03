using Content.Server.Administration;
using Content.Server.Chat.Managers;
using Content.Shared.Administration;
using Robust.Server;
using Robust.Shared.Console;

namespace Content.Server.Misfits.Administration.Commands;

/// <summary>
/// Misfits: full server restart. Announces, then shuts down cleanly so the process manager
/// (SS14 watchdog, systemd Restart=always, ...) relaunches it. IBaseServer.Restart() isn't usable for this.
/// </summary>
[AdminCommand(AdminFlags.Server)]
public sealed partial class ServerRestartCommand : IConsoleCommand
{
    [Dependency] private IBaseServer _server = default!;
    [Dependency] private IChatManager _chatManager = default!;

    public string Command => "misfitsrestart";
    public string Description => "Performs a full server restart (clean shutdown; the process manager relaunches it).";
    public string Help => "misfitsrestart";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        _chatManager.DispatchServerAnnouncement(Loc.GetString("misfits-server-restart-announcement"));
        _server.Shutdown(Loc.GetString("misfits-server-restart-shutdown-reason"));
    }
}
