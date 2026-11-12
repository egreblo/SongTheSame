using SongTheSame.Application.Abstractions;
using SongTheSame.Domain.Songs;

namespace SongTheSame.Application.Tests.Fakes;

internal sealed class InMemorySongCatalog : ISongCatalog
{
    public List<Song> Songs { get; } = [];

    public List<SongFeatures> Features { get; } = [];

    public Task<bool> ExistsAsync(long deezerId, CancellationToken cancellationToken) =>
        Task.FromResult(Songs.Any(song => song.DeezerId == deezerId));

    public Task AddAsync(Song song, SongFeatures features, CancellationToken cancellationToken)
    {
        Songs.Add(song);
        Features.Add(features);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Song>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Song>>(Songs.Where(song => ids.Contains(song.Id)).ToList());

    public Task<IReadOnlyList<Song>> GetAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Song>>(Songs.ToList());

    public Task<IReadOnlyList<Song>> SearchAsync(IReadOnlyList<string> tokens, int limit, CancellationToken cancellationToken)
    {
        IReadOnlyList<Song> result = Songs
            .Where(song => tokens.All(token => song.SearchText.Contains(token, StringComparison.Ordinal)))
            .OrderByDescending(song => song.Popularity)
            .Take(limit)
            .ToList();
        return Task.FromResult(result);
    }
}
