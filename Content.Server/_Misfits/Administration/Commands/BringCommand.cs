// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Server/_Misfits/Administration/Commands/BringCommand.cs

using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server._Misfits.Administration.Commands;

/// <summary>
/// Misfits: teleports a player or entity to the admin.
/// </summary>
[AdminCommand(AdminFlags.Admin)]
public sealed partial class BringCommand : IConsoleCommand
{
    [Dependency] private IEntityManager _entManager = default!;
    [Dependency] private IPlayerManager _playerManager = default!;

    public string Command => "bring";
    public string Description => "Teleports a player or entity to your location by entity ID, username, or character name.";
    public string Help => "bring <entity ID, username, or character name>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 1)
        {
            shell.WriteError(Help);
            return;
        }

        if (shell.Player?.AttachedEntity is not { Valid: true } admin)
        {
            shell.WriteError("You must have an attached entity to use this command.");
            return;
        }

        var name = string.Join(" ", args);
        if (FindEntity(name) is not { Valid: true } target)
        {
            shell.WriteError($"Could not find an entity with ID, username, or character name \"{name}\".");
            return;
        }

        var xform = _entManager.System<SharedTransformSystem>();
        xform.SetCoordinates(target, _entManager.GetComponent<TransformComponent>(admin).Coordinates);
        xform.AttachToGridOrMap(target);

        shell.WriteLine($"Teleported {_entManager.ToPrettyString(target)} to your location.");
    }

    private EntityUid? FindEntity(string name)
    {
        if (int.TryParse(name, out var netId) && _entManager.TryGetEntity(new NetEntity(netId), out var byId))
            return byId;

        if (_playerManager.TryGetSessionByUsername(name, out var session) && session.AttachedEntity is { Valid: true } byUser)
            return byUser;

        foreach (var player in _playerManager.Sessions)
        {
            if (player.AttachedEntity is not { Valid: true } ent)
                continue;

            if (string.Equals(_entManager.GetComponent<MetaDataComponent>(ent).EntityName, name, StringComparison.OrdinalIgnoreCase))
                return ent;
        }

        return null;
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length == 1
            ? CompletionResult.FromHintOptions(CompletionHelper.SessionNames(players: _playerManager), "<entity ID, username, or character name>")
            : CompletionResult.Empty;
    }
}
