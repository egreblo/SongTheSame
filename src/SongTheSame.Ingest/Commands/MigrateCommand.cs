using Microsoft.EntityFrameworkCore;
using SongTheSame.Infrastructure.Persistence;

namespace SongTheSame.Ingest.Commands;

public sealed class MigrateCommand(AppDbContext db)
{
    public async Task<int> RunAsync()
    {
        await db.Database.MigrateAsync();
        Console.WriteLine("Database is up to date.");
        return 0;
    }
}
