using Content.Server.Administration.Managers;
using Content.Shared.Administration;
using Content.Shared.Follower;
using Content.Shared.Ghost.Components;
using Robust.Server.Console;
using Robust.Shared.Console;

namespace Content.Server.Misfits.Administration.Commands;

/// <summary>
/// Misfits: makes the caller's ghost follow an entity. Admins who aren't ghosts are aghosted first.
/// </summary>
[AnyCommand]
public sealed partial class GhostFollowEntityCommand : IConsoleCommand
{
    [Dependency] private IEntityManager _entManager = default!;
    [Dependency] private IAdminManager _adminManager = default!;
    [Dependency] private IServerConsoleHost _consoleHost = default!;

    public string Command => "ghostfollow";
    public string Description => "Makes your ghost follow the given entity.";
    public string Help => "ghostfollow <net entity id>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player is not { } player)
        {
            shell.WriteError("Only players can use this command.");
            return;
        }

        if (args.Length != 1)
        {
            shell.WriteError(Help);
            return;
        }

        if (!NetEntity.TryParse(args[0], out var netEnt) || !_entManager.TryGetEntity(netEnt, out var target))
        {
            shell.WriteError($"Could not find entity with ID '{args[0]}'.");
            return;
        }

        if (player.AttachedEntity is not { } follower || !_entManager.HasComponent<GhostComponent>(follower))
        {
            if (!_adminManager.HasAdminFlag(player, AdminFlags.Admin))
            {
                shell.WriteError("You must be a ghost to use this command.");
                return;
            }

            _consoleHost.ExecuteCommand(player, "aghost");
            if (player.AttachedEntity is not { } aghost || !_entManager.HasComponent<GhostComponent>(aghost))
            {
                shell.WriteError("You must be a ghost to use this command.");
                return;
            }

            follower = aghost;
        }

        _entManager.System<FollowerSystem>().StartFollowingEntity(follower, target.Value);
    }
}
