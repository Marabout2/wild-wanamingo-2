using Content.IntegrationTests.Fixtures;
using Content.Server.Database;
using Content.Server.Preferences.Managers;
using Content.Shared._Misfits.Special;
using Content.Shared.Preferences;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Robust.Shared.Configuration;
using Robust.Shared.Log;
using Robust.Shared.Maths;
using Robust.Shared.Network;
using Robust.Shared.Serialization.Manager;
using Robust.UnitTesting;

namespace Content.IntegrationTests.Tests._WW;

/// <summary>
/// Character SPECIAL stats survive a save and load, and the hand-written ProfileSpecial migrations match the model.
/// </summary>
[TestFixture]
public sealed class SpecialProfileDbTest : GameTest
{
    private static ServerDbSqlite GetDb(RobustIntegrationTest.ServerIntegrationInstance server)
    {
        var cfg = server.ResolveDependency<IConfigurationManager>();
        var serialization = server.ResolveDependency<ISerializationManager>();
        var opsLog = server.ResolveDependency<ILogManager>().GetSawmill("db.ops");
        var builder = new DbContextOptionsBuilder<SqliteServerDbContext>();
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        builder.UseSqlite(conn);
        return new ServerDbSqlite(() => builder.Options, true, cfg, true, opsLog, serialization, false);
    }

    [Test]
    public async Task SpecialSavesAndLoads()
    {
        var pair = Pair;
        var db = GetDb(pair.Server);
        var preferences = (ServerPreferencesManager) pair.Server.ResolveDependency<IServerPreferencesManager>();
        var user = new NetUserId(new Guid("2b4f8f5e-1c0a-4d33-9a1e-6f0d2b7c9a11"));

        // 40 points: the default 35 plus the full 5-point bonus pool.
        var special = new SpecialProfile
        {
            Strength = 9,
            Perception = 3,
            Endurance = 6,
            Charisma = 2,
            Intelligence = 8,
            Agility = 7,
            Luck = 5,
        };
        Assert.That(special.IsValid);

        var profile = new HumanoidCharacterProfile
        {
            Name = "Vault Dweller",
            Species = "Human",
            Age = 30,
            Appearance = new(Color.Azure, Color.Beige, new()),
        }.WithSpecial(special);

        await db.InitPrefsAsync(user, profile);
        var prefs = await db.GetPlayerPreferencesAsync(user);
        var loaded = preferences.ConvertProfiles(prefs!.Profiles.Find(p => p.Slot == 0));

        Assert.That(loaded.Special, Is.EqualTo(special));
        Assert.That(loaded.MemberwiseEquals(profile));
    }

    [Test]
    public void OverBudgetSpecialResets()
    {
        var cheat = new SpecialProfile { Strength = 10, Perception = 10, Endurance = 10 };
        Assert.That(cheat.IsValid, Is.False);
        Assert.That(SpecialProfile.EnsureValid(cheat), Is.EqualTo(SpecialProfile.Default()));
    }

    [Test]
    public void SqliteMigrationsMatchModel()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        var options = new DbContextOptionsBuilder<SqliteServerDbContext>().UseSqlite(conn).Options;
        using var context = new SqliteServerDbContext(options);
        Assert.That(context.Database.HasPendingModelChanges(), Is.False);
    }

    [Test]
    public void PostgresMigrationsMatchModel()
    {
        // No server needed: the model comparison doesn't open the connection.
        var options = new DbContextOptionsBuilder<PostgresServerDbContext>()
            .UseNpgsql("Host=localhost;Database=unused")
            .Options;
        using var context = new PostgresServerDbContext(options);
        Assert.That(context.Database.HasPendingModelChanges(), Is.False);
    }
}
