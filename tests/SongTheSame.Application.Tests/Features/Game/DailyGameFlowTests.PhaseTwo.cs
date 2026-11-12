using SongTheSame.Application.Features.Game.Common;
using SongTheSame.Application.Features.Game.GuessSong;
using SongTheSame.Application.Features.Game.RefreshMatches;
using SongTheSame.Application.Features.Game.SkipSimilarity;
using SongTheSame.Application.Features.Game.StartSession;
using SongTheSame.Application.Features.Game.SubmitSimilarity;
using SongTheSame.Application.Tests.Fakes;
using SongTheSame.Domain.Game;

namespace SongTheSame.Application.Tests.Features.Game;

/// <summary>Phase 2 (finding a twin), skipping it, and refreshing stored twins. Shares the fixture of the daily flow tests.</summary>
public partial class DailyGameFlowTests
{
    [Fact]
    public async Task Phase_two_scores_between_1_and_100_and_shows_the_best_matches_afterwards()
    {
        var view = await _start.HandleAsync(new StartSessionCommand(Player), Ct);
        var target = TargetOf(view.SessionId, 0);
        await _guess.HandleAsync(new GuessSongCommand(view.SessionId, Player, 0, target.Id), Ct);
        var other = _catalog.Songs.First(song => song.Id != target.Id);

        view = await _similarity.HandleAsync(new SubmitSimilarityCommand(view.SessionId, Player, 0, other.Id), Ct);

        var phase2 = view.Rounds[0].Phase2!;
        phase2.Score.ShouldBeInRange(1, 100);
        phase2.Points.ShouldBeInRange(0, 50);
        phase2.BestMatches.Count.ShouldBe(3);
        phase2.BestMatches[0].Score.ShouldBe(100);
        view.Rounds[0].Status.ShouldBe(RoundStatus.Done);
        view.Rounds[1].Status.ShouldBe(RoundStatus.Phase1);
    }

    [Fact]
    public async Task Choosing_the_best_match_in_phase_two_earns_the_maximum_score()
    {
        var view = await _start.HandleAsync(new StartSessionCommand(Player), Ct);
        var session = _sessions.Sessions.Single();
        var puzzle = _puzzles.Puzzles.Single();
        var target = TargetOf(view.SessionId, 0);
        await _guess.HandleAsync(new GuessSongCommand(view.SessionId, Player, 0, target.Id), Ct);
        var bestId = puzzle.Rounds[0].BestMatches.MaxBy(match => match.Score)!.SongId;

        view = await _similarity.HandleAsync(new SubmitSimilarityCommand(session.Id, Player, 0, bestId), Ct);

        view.Rounds[0].Phase2!.Score.ShouldBe(100);
        view.Rounds[0].Phase2!.Points.ShouldBe(50);
    }

    [Fact]
    public async Task The_view_tells_where_each_twin_is_most_similar_so_it_can_be_played_from_there()
    {
        var view = await _start.HandleAsync(new StartSessionCommand(Player), Ct);
        var target = TargetOf(view.SessionId, 0);
        await _guess.HandleAsync(new GuessSongCommand(view.SessionId, Player, 0, target.Id), Ct);
        var other = _catalog.Songs.First(song => song.Id != target.Id);

        view = await _similarity.HandleAsync(new SubmitSimilarityCommand(view.SessionId, Player, 0, other.Id), Ct);

        var phase2 = view.Rounds[0].Phase2!;
        phase2.GuessMatchStartSeconds.ShouldNotBeNull();
        phase2.GuessMatchStartSeconds!.Value.ShouldBeInRange(0, 30);
        phase2.BestMatches.ShouldAllBe(match => match.StartSeconds.HasValue && match.StartSeconds.Value >= 0);
    }

    [Fact]
    public async Task Skipping_phase_two_scores_zero_shows_the_twins_and_opens_the_next_round()
    {
        var view = await _start.HandleAsync(new StartSessionCommand(Player), Ct);
        var target = TargetOf(view.SessionId, 0);
        await _guess.HandleAsync(new GuessSongCommand(view.SessionId, Player, 0, target.Id), Ct);
        var skip = new SkipSimilarityHandler(new SessionLoader(_sessions, _puzzles), _sessions,
            new SessionViewBuilder(_catalog, new FakeMusicSource()), _clock);

        view = await skip.HandleAsync(new SkipSimilarityCommand(view.SessionId, Player, 0), Ct);

        var phase2 = view.Rounds[0].Phase2!;
        phase2.Skipped.ShouldBeTrue();
        phase2.Guess.ShouldBeNull();
        phase2.Score.ShouldBe(0);
        phase2.Points.ShouldBe(0);
        phase2.BestMatches.Count.ShouldBe(3);
        view.Rounds[0].Status.ShouldBe(RoundStatus.Done);
        view.Rounds[1].Status.ShouldBe(RoundStatus.Phase1);
        view.TotalPoints.ShouldBe(50);
    }

    [Fact]
    public async Task Refreshing_recomputes_the_twins_of_existing_puzzles_without_touching_games()
    {
        var view = await _start.HandleAsync(new StartSessionCommand(Player), Ct);
        var round = _puzzles.Puzzles.Single().Rounds[0];
        round.ReplaceBestMatches([]);

        var refreshed = await new RefreshMatchesHandler(_puzzles, _catalog, _builder)
            .HandleAsync(new RefreshMatchesCommand(new DateOnly(2026, 11, 13)), Ct);

        refreshed.ShouldBe(5);
        round.BestMatches.Count.ShouldBe(3);
        round.BestMatches.ShouldAllBe(match => match.StartSeconds.HasValue);
        (await _sessions.GetAsync(view.SessionId, Ct)).ShouldNotBeNull();
    }

    [Fact]
    public async Task Phase_two_rejects_the_song_from_phase_one()
    {
        var view = await _start.HandleAsync(new StartSessionCommand(Player), Ct);
        var target = TargetOf(view.SessionId, 0);
        await _guess.HandleAsync(new GuessSongCommand(view.SessionId, Player, 0, target.Id), Ct);

        await Should.ThrowAsync<GameRuleException>(
            () => _similarity.HandleAsync(new SubmitSimilarityCommand(view.SessionId, Player, 0, target.Id), Ct));
    }

    [Fact]
    public async Task Phase_two_is_refused_before_phase_one_is_finished()
    {
        var view = await _start.HandleAsync(new StartSessionCommand(Player), Ct);
        var other = _catalog.Songs[0];

        await Should.ThrowAsync<GameRuleException>(
            () => _similarity.HandleAsync(new SubmitSimilarityCommand(view.SessionId, Player, 0, other.Id), Ct));
    }
}
