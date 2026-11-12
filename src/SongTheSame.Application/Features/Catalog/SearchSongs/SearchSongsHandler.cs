using SongTheSame.Application.Abstractions;
using SongTheSame.Domain.Songs;

namespace SongTheSame.Application.Features.Catalog.SearchSongs;

public sealed class SearchSongsHandler(ISongCatalog catalog)
    : IHandler<SearchSongsQuery, IReadOnlyList<SongSummary>>
{
    private const int MinimumQueryLength = 2;
    private const int MaximumLimit = 20;

    public async Task<IReadOnlyList<SongSummary>> HandleAsync(SearchSongsQuery request, CancellationToken cancellationToken)
    {
        var tokens = SearchText.Tokenize(request.Text);
        if (tokens.Sum(token => token.Length) < MinimumQueryLength) return [];

        var limit = Math.Clamp(request.Limit, 1, MaximumLimit);
        var songs = await catalog.SearchAsync(tokens, limit, cancellationToken);
        return songs.Select(song => new SongSummary(song.Id, song.Title, song.Artist, song.CoverUrl)).ToList();
    }
}
