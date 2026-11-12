using System.Globalization;
using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Catalog.SearchSongs;
using SongTheSame.Application.Features.Game.Common;
using SongTheSame.Domain.Game;
using SongTheSame.Domain.Puzzles;
using SongTheSame.Domain.Reports;
using SongTheSame.Domain.Songs;

namespace SongTheSame.Application.Features.Admin.ListReports;

public sealed record ListReportsQuery(ReportStatus Status, int Limit = 50);

public sealed class ListReportsHandler(
    IReportRepository reports,
    ISessionRepository sessions,
    IPuzzleRepository puzzles,
    ISongCatalog catalog) : IHandler<ListReportsQuery, IReadOnlyList<ReportItem>>
{
    public async Task<IReadOnlyList<ReportItem>> HandleAsync(ListReportsQuery request, CancellationToken cancellationToken)
    {
        var found = await reports.ListAsync(request.Status, Math.Clamp(request.Limit, 1, 200), cancellationToken);
        if (found.Count == 0) return [];

        var sessionList = await sessions.GetManyAsync(found.Select(r => r.SessionId).Distinct().ToList(), cancellationToken);
        var sessionsById = sessionList.ToDictionary(session => session.Id);
        var puzzleList = await puzzles.GetManyAsync(sessionList.Select(s => s.PuzzleId).Distinct().ToList(), cancellationToken);
        var puzzlesById = puzzleList.ToDictionary(puzzle => puzzle.Id);

        var songIds = found.SelectMany(r => new[] { r.TargetSongId, r.GuessedSongId })
            .Concat(puzzleList.SelectMany(p => p.Rounds.SelectMany(round => round.BestMatches.Select(m => m.SongId))))
            .Distinct().ToList();
        var songs = (await catalog.GetByIdsAsync(songIds, cancellationToken)).ToDictionary(song => song.Id);

        return found
            .Where(report => sessionsById.ContainsKey(report.SessionId))
            .Select(report => ToItem(report, sessionsById[report.SessionId], puzzlesById, songs))
            .ToList();
    }

    private static ReportItem ToItem(
        RoundReport report, GameSession session, Dictionary<Guid, Puzzle> puzzlesById, Dictionary<Guid, Song> songs)
    {
        var puzzle = puzzlesById[session.PuzzleId];
        var round = puzzle.Rounds.Single(r => r.Index == report.RoundIndex);
        var progress = session.Rounds.Single(r => r.Index == report.RoundIndex);
        var bestScore = round.BestMatches.Count > 0 ? round.BestMatches.Max(m => m.Score) : 0;

        var matches = round.BestMatches
            .Select(match => new MatchView(Summarize(songs[match.SongId]), RoundScoring.RelativeToBest(match.Score, bestScore), match.StartSeconds))
            .ToList();

        return new ReportItem(
            report.Id, report.CreatedAt, session.Day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), report.RoundIndex,
            report.Status, report.Note, report.AdminNote, report.Score,
            Summarize(songs[report.TargetSongId]), round.SegmentStartSeconds, round.SegmentSeconds,
            Summarize(songs[report.GuessedSongId]), progress.Phase2MatchStartSeconds, matches);
    }

    private static SongSummary Summarize(Song song) => new(song.Id, song.Title, song.Artist, song.CoverUrl);
}
