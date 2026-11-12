using SongTheSame.Domain.Songs;

namespace SongTheSame.Application.Abstractions;

public interface ISongCatalog
{
    Task<bool> ExistsAsync(long deezerId, CancellationToken cancellationToken);

    Task AddAsync(Song song, SongFeatures features, CancellationToken cancellationToken);

    Task<IReadOnlyList<Song>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);

    /// <summary>The whole catalog without audio features. Small enough to load for batch work.</summary>
    Task<IReadOnlyList<Song>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>Songs whose search text contains every token, most popular first.</summary>
    Task<IReadOnlyList<Song>> SearchAsync(IReadOnlyList<string> tokens, int limit, CancellationToken cancellationToken);
}
