namespace SongTheSame.Application.Features.CustomGames;

public sealed record CustomGameSummary(string Code, string Title, DateTimeOffset CreatedAt);

public sealed record CustomGameInfo(string Code, string Title, string? AuthorName);
