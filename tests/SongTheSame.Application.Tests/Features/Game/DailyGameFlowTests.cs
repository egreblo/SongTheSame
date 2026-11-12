using Microsoft.Extensions.Time.Testing;
using SongTheSame.Application.Features.Game.Common;
using SongTheSame.Application.Features.Game.GetSession;
using SongTheSame.Application.Features.Game.GuessSong;
using SongTheSame.Application.Features.Game.RefreshMatches;
using SongTheSame.Application.Features.Game.SkipClip;
using SongTheSame.Application.Features.Game.SkipSimilarity;
using SongTheSame.Application.Features.Game.StartSession;
using SongTheSame.Application.Features.Game.SubmitSimilarity;
using SongTheSame.Application.Features.Similarity;
using SongTheSame.Application.Tests.Fakes;
using SongTheSame.Domain.Game;
using SongTheSame.Domain.Similarity;
using SongTheSame.Domain.Songs;

namespace SongTheSame.Application.Tests.Features.Game;

/// <summary>Plays whole days through the real handlers with in-memory stores and synthetic audio features.</summary>
public partial class DailyGameFlowTests
{
    private const string Player = "anon:11111111-1111-1111-1111-111111111111";

    private readonly InMemorySongCatalog _catalog = new();
    private readonly InMemoryPuzzleRepository _puzzles = new();
    private readonly InMemorySessionRepository _sessions = new();
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 11, 13, 10, 0, 0, TimeSpan.Zero));
    private readonly PuzzleRoundBuilder _builder;
    private readonly StartSessionHandler _start;
    private readonly GuessSongHandler _guess;
    private readonly SkipClipHandler _skip;
    private readonly SubmitSimilarityHandler _similarity;
    private readonly GetSessionHandler _get;

    public DailyGameFlowTests()
    {
        var features = new InMemoryFeatureStore();
        for (var i = 1; i <= 40; i++)
        {
            var song = Song.Create(i, $"Song {i}", $"Artist {i}", null, null, 200, 1000 - i,
                i % 4 == 0 ? SongOrigin.Regional : SongOrigin.International, _clock.GetUtcNow());
            _catalog.Songs.Add(song);
            features.Add(song.Id, seed: i);
        }

        var calibrations = new InMemoryCalibrationStore();
        calibrations.SaveAsync(
            SimilarityCalibration.Create(1, Enumerable.Range(0, 500).Select(i => 0.1f + i / 1000f), _clock.GetUtcNow()),
            CancellationToken.None);

        var similarity = new SimilarityService(features, calibrations);
        _builder = new PuzzleRoundBuilder(features, similarity);
        var provider = new DailyPuzzleProvider(_puzzles, _catalog, _builder, _clock);
        var views = new SessionViewBuilder(_catalog, new FakeMusicSource());
        var loader = new SessionLoader(_sessions, _puzzles);

        _start = new StartSessionHandler(provider, new SessionStarter(_sessions, views, _clock), _clock);
        _get = new GetSessionHandler(loader, views);
        _guess = new GuessSongHandler(loader, _sessions, _catalog, views);
        _skip = new SkipClipHandler(loader, _sessions, views);
        _similarity = new SubmitSimilarityHandler(loader, _sessions, _catalog, similarity, views, _clock);
    }

    private Song TargetOf(Guid sessionId, int round)
    {
        var session = _sessions.Sessions.Single(s => s.Id == sessionId);
        var puzzle = _puzzles.Puzzles.Single(p => p.Id == session.PuzzleId);
        var songId = puzzle.Rounds.Single(r => r.Index == round).SongId;
        return _catalog.Songs.Single(song => song.Id == songId);
    }

    private static CancellationToken Ct => CancellationToken.None;

    [Fact]
    public async Task Starting_twice_returns_the_same_session_and_puzzle()
    {
        var first = await _start.HandleAsync(new StartSessionCommand(Player), Ct);
        var second = await _start.HandleAsync(new StartSessionCommand(Player), Ct);

        second.SessionId.ShouldBe(first.SessionId);
        _puzzles.Puzzles.Count.ShouldBe(1);
        first.Rounds.Count.ShouldBe(5);
        first.Day.ShouldBe("2026-11-13");
    }

    [Fact]
    public async Task Only_the_first_round_is_open_and_nothing_is_revealed_at_the_start()
    {
        var view = await _start.HandleAsync(new StartSessionCommand(Player), Ct);

        view.Rounds[0].Status.ShouldBe(RoundStatus.Phase1);
        view.Rounds[0].AudioUrl.ShouldNotBeNull();
        view.Rounds[0].Reveal.ShouldBeNull();
        view.Rounds.Skip(1).ShouldAllBe(round => round.Status == RoundStatus.Locked && round.AudioUrl == null);
    }

    [Fact]
    public async Task A_correct_first_guess_reveals_the_song_and_opens_phase_two()
    {
        var view = await _start.HandleAsync(new StartSessionCommand(Player), Ct);
        var target = TargetOf(view.SessionId, 0);

        view = await _guess.HandleAsync(new GuessSongCommand(view.SessionId, Player, 0, target.Id), Ct);

        view.Rounds[0].Status.ShouldBe(RoundStatus.Phase2);
        view.Rounds[0].Reveal!.Solved.ShouldBeTrue();
        view.Rounds[0].Reveal!.Song.Id.ShouldBe(target.Id);
        view.Rounds[0].Phase1Points.ShouldBe(50);
    }

    [Fact]
    public async Task Another_edition_of_the_same_song_counts_as_correct()
    {
        var view = await _start.HandleAsync(new StartSessionCommand(Player), Ct);
        var target = TargetOf(view.SessionId, 0);
        var remaster = Song.Create(
            999, $"{target.Title} (Remastered 2011)", target.Artist, null, null, 200, 1, SongOrigin.International, _clock.GetUtcNow());
        _catalog.Songs.Add(remaster);

        view = await _guess.HandleAsync(new GuessSongCommand(view.SessionId, Player, 0, remaster.Id), Ct);

        view.Rounds[0].Reveal!.Solved.ShouldBeTrue();
    }

    [Fact]
    public async Task Wrong_guesses_and_skips_unlock_longer_clips_until_attempts_run_out()
    {
        var view = await _start.HandleAsync(new StartSessionCommand(Player), Ct);
        var wrong = _catalog.Songs.First(song => song.Id != TargetOf(view.SessionId, 0).Id);

        view = await _guess.HandleAsync(new GuessSongCommand(view.SessionId, Player, 0, wrong.Id), Ct);
        view.Rounds[0].UnlockedClipSeconds.ShouldBe(2);
        view.Rounds[0].Guesses.ShouldHaveSingleItem().Song!.Id.ShouldBe(wrong.Id);

        for (var i = 1; i < GameRules.MaxPhase1Attempts; i++)
            view = await _skip.HandleAsync(new SkipClipCommand(view.SessionId, Player, 0), Ct);

        view.Rounds[0].Status.ShouldBe(RoundStatus.Phase2);
        view.Rounds[0].Reveal!.Solved.ShouldBeFalse();
        view.Rounds[0].Phase1Points.ShouldBe(0);
    }

    [Fact]
    public async Task Another_player_cannot_see_or_play_the_session()
    {
        var view = await _start.HandleAsync(new StartSessionCommand(Player), Ct);
        const string intruder = "anon:22222222-2222-2222-2222-222222222222";

        await Should.ThrowAsync<KeyNotFoundException>(() => _get.HandleAsync(new GetSessionQuery(view.SessionId, intruder), Ct));
        await Should.ThrowAsync<KeyNotFoundException>(
            () => _skip.HandleAsync(new SkipClipCommand(view.SessionId, intruder, 0), Ct));
    }

    [Fact]
    public async Task A_full_perfect_day_scores_500_and_completes_the_session()
    {
        var view = await _start.HandleAsync(new StartSessionCommand(Player), Ct);
        var puzzle = _puzzles.Puzzles.Single();

        for (var round = 0; round < GameRules.RoundsPerDay; round++)
        {
            var target = TargetOf(view.SessionId, round);
            await _guess.HandleAsync(new GuessSongCommand(view.SessionId, Player, round, target.Id), Ct);
            var best = puzzle.Rounds[round].BestMatches.MaxBy(match => match.Score)!.SongId;
            view = await _similarity.HandleAsync(new SubmitSimilarityCommand(view.SessionId, Player, round, best), Ct);
        }

        view.Completed.ShouldBeTrue();
        view.TotalPoints.ShouldBe(500);
    }

    [Fact]
    public async Task The_next_day_gets_a_different_puzzle_without_repeating_songs()
    {
        await _start.HandleAsync(new StartSessionCommand(Player), Ct);
        _clock.Advance(TimeSpan.FromDays(1));

        var next = await _start.HandleAsync(new StartSessionCommand(Player), Ct);

        _puzzles.Puzzles.Count.ShouldBe(2);
        next.Day.ShouldBe("2026-11-14");
        _puzzles.Puzzles[1].Rounds.Select(r => r.SongId).Intersect(_puzzles.Puzzles[0].Rounds.Select(r => r.SongId)).ShouldBeEmpty();
    }
}
