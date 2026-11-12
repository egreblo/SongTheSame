namespace SongTheSame.Infrastructure.Tests.Persistence;

/// <summary>
/// Runs only when SONGTHESAME_TEST_DB holds a connection string to a PostgreSQL server where a
/// throwaway database may be created (the user needs CREATEDB). Otherwise the test is skipped.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class RequiresDatabaseFactAttribute : FactAttribute
{
    public const string EnvironmentVariable = "SONGTHESAME_TEST_DB";

    public RequiresDatabaseFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(EnvironmentVariable)))
            Skip = $"Set {EnvironmentVariable} to a PostgreSQL connection string to run database tests.";
    }
}
