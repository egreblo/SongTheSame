using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SongTheSame.Application.Abstractions;
using SongTheSame.Audio;
using SongTheSame.Domain.Songs;

namespace SongTheSame.Infrastructure.Persistence;

internal sealed class SongFeatureStore(AppDbContext db, IMemoryCache cache) : ISongFeatureStore
{
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromHours(6);

    public async Task<ChromaSequence?> GetAsync(Guid songId, CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(songId, out ChromaSequence? cached)) return cached;

        var row = await db.SongFeatures.AsNoTracking().FirstOrDefaultAsync(f => f.SongId == songId, cancellationToken);
        if (row is null) return null;

        var chroma = Decode(row);
        cache.Set(songId, chroma, CacheLifetime);
        return chroma;
    }

    public async Task<IReadOnlyList<SongChroma>> GetAllAsync(CancellationToken cancellationToken)
    {
        var rows = await db.SongFeatures.AsNoTracking().ToListAsync(cancellationToken);
        return rows.Select(row => new SongChroma(row.SongId, Decode(row))).ToList();
    }

    private static ChromaSequence Decode(SongFeatures row) => ChromaCodec.Decode(row.Data, row.FramesPerSecond);
}
