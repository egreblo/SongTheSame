using Microsoft.EntityFrameworkCore;
using Npgsql;
using SongTheSame.Infrastructure.Persistence;

namespace SongTheSame.Infrastructure.Tests.Persistence;

/// <summary>Creates a uniquely named database, applies all migrations, and drops it on dispose.</summary>
public sealed class TemporaryDatabase : IAsyncDisposable
{
    private readonly NpgsqlConnectionStringBuilder _admin;
    private readonly string _name = $"songthesame_test_{Guid.NewGuid():N}";

    private TemporaryDatabase(NpgsqlConnectionStringBuilder admin) => _admin = admin;

    public string ConnectionString =>
        new NpgsqlConnectionStringBuilder(_admin.ConnectionString) { Database = _name }.ConnectionString;

    public static async Task<TemporaryDatabase> CreateAsync()
    {
        var admin = new NpgsqlConnectionStringBuilder(
            Environment.GetEnvironmentVariable(RequiresDatabaseFactAttribute.EnvironmentVariable));
        var database = new TemporaryDatabase(admin);

        await using (var connection = new NpgsqlConnection(admin.ConnectionString))
        {
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand($"CREATE DATABASE \"{database._name}\"", connection);
            await command.ExecuteNonQueryAsync();
        }

        await using var context = database.NewContext();
        await context.Database.MigrateAsync();
        return database;
    }

    public AppDbContext NewContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(ConnectionString).Options);

    public async ValueTask DisposeAsync()
    {
        NpgsqlConnection.ClearAllPools();
        await using var connection = new NpgsqlConnection(_admin.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{_name}\" WITH (FORCE)", connection);
        await command.ExecuteNonQueryAsync();
    }
}
