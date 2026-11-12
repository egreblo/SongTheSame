namespace SongTheSame.Application.Features.Catalog.SearchSongs;

public sealed record SearchSongsQuery(string Text, int Limit = 8);

public sealed record SongSummary(Guid Id, string Title, string Artist, string? CoverUrl);
