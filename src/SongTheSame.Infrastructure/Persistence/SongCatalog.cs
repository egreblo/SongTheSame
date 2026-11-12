using Microsoft.EntityFrameworkCore;
using SongTheSame.Application.Abstractions;
using SongTheSame.Domain.Songs;

namespace SongTheSame.Infrastructure.Persistence;

internal sealed class SongCatalog(AppDbContext db) : ISongCatalog
{
    public Task<bool> ExistsAsync(long deezerId, CancellationToken cancellationToken) =>
        db.Songs.AnyAsync(song => song.DeezerId == deezerId, cancellationToken);

    public async Task AddAsync(Song song, SongFeatures features, CancellationToken cancellationToken)
    {
        db.Songs.Add(song);
        db.SongFeatures.Add(features);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Song>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken) =>
        await db.Songs.AsNoTracking().Where(song => ids.Contains(song.Id)).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Song>> GetAllAsync(CancellationToken cancellationToken) =>
        await db.Songs.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Song>> SearchAsync(
        IReadOnlyList<string> tokens, int limit, CancellationToken cancellationToken)
    {
        IQueryable<Song> query = db.Songs.AsNoTracking();
        foreach (var token in tokens)
        {
            var pattern = $"%{EscapeLikePattern(token)}%";
            query = query.Where(song => EF.Functions.ILike(song.SearchText, pattern));
        }

        return await query
            .OrderByDescending(song => song.Popularity)
            .ThenBy(song => song.Title)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    private static string EscapeLikePattern(string token) =>
        token.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
}
