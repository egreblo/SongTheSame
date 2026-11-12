using SongTheSame.Application.Features.Catalog.SearchSongs;
using SongTheSame.Application.Features.Game.Common;
using SongTheSame.Domain.Reports;

namespace SongTheSame.Application.Features.Admin;

/// <summary>Everything an admin needs to judge a reported score, including where to listen.</summary>
public sealed record ReportItem(
    Guid Id,
    DateTimeOffset CreatedAt,
    string Day,
    int RoundIndex,
    ReportStatus Status,
    string? PlayerNote,
    string? AdminNote,
    int Score,
    SongSummary Target,
    double SegmentStartSeconds,
    double SegmentSeconds,
    SongSummary Guess,
    double? GuessMatchStartSeconds,
    IReadOnlyList<MatchView> BestMatches);
