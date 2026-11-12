using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace SongTheSame.Infrastructure.Persistence;

internal static class UniqueViolation
{
    public static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
