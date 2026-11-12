using System.Globalization;
using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Catalog.SearchSongs;
using SongTheSame.Domain.Game;
using SongTheSame.Domain.Puzzles;
using SongTheSame.Domain.Songs;

namespace SongTheSame.Application.Features.Game.Common;

/// <summary>Builds what the player is allowed to see: answers appear only after the phase that earns them.</summary>
public sealed class SessionViewBuilder(ISongCatalog catalog, IMusicSource music)
{
    public async Task<SessionView> BuildAsync(GameSession session, Puzzle puzzle, CancellationToken cancellationToken)
    {
        var songs = await LoadSongsAsync(session, puzzle, cancellationToken);
        var rounds = new List<RoundView>();
        foreach (var progress in session.Rounds.OrderBy(round => round.Index))
        {
            var round = puzzle.Rounds.Single(r => r.Index == progress.Index);
            var locked = session.Rounds.Any(earlier => earlier.Index < progress.Index && !earlier.Phase2Done);
            rounds.Add(await BuildRoundAsync(progress, round, locked, songs, cancellationToken));
        }

        return new SessionView(
            session.Id, session.Day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            session.TotalPoints, GameRules.MaxDailyPoints, session.IsCompleted, rounds);
    }

    private async Task<RoundView> BuildRoundAsync(
        RoundProgress progress, PuzzleRound round, bool locked, Dictionary<Guid, Song> songs, CancellationToken cancellationToken)
    {
        var status = locked ? RoundStatus.Locked
            : progress.Phase2Done ? RoundStatus.Done
            : progress.Phase1Done ? RoundStatus.Phase2
            : RoundStatus.Phase1;

        var target = songs[round.SongId];
        var audioUrl = locked ? null : await music.GetPreviewUrlAsync(target.DeezerId, cancellationToken);
        var guesses = progress.Phase1Guesses
            .Select(id => new GuessView(id == Guid.Empty ? null : Summarize(songs[id])))
            .ToList();

        var reveal = progress.Phase1Done
            ? new RevealView(Summarize(target), progress.Phase1Solved, round.SegmentStartSeconds, round.SegmentSeconds)
            : null;

        var phase2 = progress.Phase2Done
            ? new Phase2View(
                progress.Phase2SongId is { } guessId ? Summarize(songs[guessId]) : null,
                progress.Phase2Skipped, progress.Phase2Score!.Value, progress.Phase2Points, progress.Phase2MatchStartSeconds,
                round.BestMatches
                    .Select(match => new MatchView(
                        Summarize(songs[match.SongId]),
                        RoundScoring.RelativeToBest(match.Score, round.BestMatches.Max(m => m.Score)),
                        match.StartSeconds))
                    .ToList())
            : null;

        return new RoundView(
            progress.Index, status, audioUrl, progress.Phase1Attempts, GameRules.MaxPhase1Attempts,
            progress.UnlockedClipSeconds, GameRules.ClipSeconds, guesses, reveal, phase2, progress.Phase1Points, progress.Points);
    }

    private async Task<Dictionary<Guid, Song>> LoadSongsAsync(GameSession session, Puzzle puzzle, CancellationToken cancellationToken)
    {
        var ids = puzzle.Rounds.Select(round => round.SongId)
            .Concat(puzzle.Rounds.SelectMany(round => round.BestMatches.Select(match => match.SongId)))
            .Concat(session.Rounds.SelectMany(round => round.Phase1Guesses))
            .Concat(session.Rounds.Where(round => round.Phase2SongId.HasValue).Select(round => round.Phase2SongId!.Value))
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();
        var songs = await catalog.GetByIdsAsync(ids, cancellationToken);
        return songs.ToDictionary(song => song.Id);
    }

    private static SongSummary Summarize(Song song) => new(song.Id, song.Title, song.Artist, song.CoverUrl);
}
