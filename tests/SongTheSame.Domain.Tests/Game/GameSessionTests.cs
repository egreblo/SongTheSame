using SongTheSame.Domain.Game;
using SongTheSame.Domain.Puzzles;

namespace SongTheSame.Domain.Tests.Game;

public class GameSessionTests
{
    private static GameSession NewSession()
    {
        var rounds = Enumerable.Range(0, GameRules.RoundsPerDay)
            .Select(i => PuzzleRound.Create(i, Guid.NewGuid(), 50, 5, 6, []));
        var puzzle = Puzzle.CreateDaily(new DateOnly(2026, 11, 13), rounds, DateTimeOffset.UnixEpoch);
        return GameSession.Start("anon:test", puzzle, DateTimeOffset.UnixEpoch);
    }

    [Fact]
    public void Solving_on_the_first_attempt_gives_full_phase1_points_and_ends_phase1()
    {
        var session = NewSession();

        session.RegisterPhase1Attempt(0, Guid.NewGuid(), correct: true);

        var round = session.Rounds[0];
        round.Phase1Done.ShouldBeTrue();
        round.Phase1Points.ShouldBe(50);
    }

    [Fact]
    public void Each_wrong_attempt_unlocks_a_longer_clip()
    {
        var session = NewSession();
        var seconds = new List<int> { session.Rounds[0].UnlockedClipSeconds };

        for (var i = 0; i < 3; i++)
        {
            session.RegisterPhase1Attempt(0, Guid.NewGuid(), correct: false);
            seconds.Add(session.Rounds[0].UnlockedClipSeconds);
        }

        seconds.ShouldBe([1, 2, 4, 7]);
    }

    [Fact]
    public void Running_out_of_attempts_ends_phase1_unsolved()
    {
        var session = NewSession();

        for (var i = 0; i < GameRules.MaxPhase1Attempts; i++)
            session.RegisterPhase1Attempt(0, Guid.Empty, correct: false);

        session.Rounds[0].Phase1Done.ShouldBeTrue();
        session.Rounds[0].Phase1Solved.ShouldBeFalse();
        session.Rounds[0].Phase1Points.ShouldBe(0);
    }

    [Fact]
    public void Phase2_is_locked_until_phase1_is_done()
    {
        var session = NewSession();

        Should.Throw<GameRuleException>(() => session.RegisterPhase2(0, Guid.NewGuid(), 80, DateTimeOffset.UnixEpoch));
    }

    [Fact]
    public void Rounds_must_be_played_in_order()
    {
        var session = NewSession();

        Should.Throw<GameRuleException>(() => session.RegisterPhase1Attempt(1, Guid.NewGuid(), correct: true));
    }

    [Fact]
    public void Cannot_guess_again_after_phase1_is_finished()
    {
        var session = NewSession();
        session.RegisterPhase1Attempt(0, Guid.NewGuid(), correct: true);

        Should.Throw<GameRuleException>(() => session.RegisterPhase1Attempt(0, Guid.NewGuid(), correct: true));
    }

    [Fact]
    public void Playing_all_five_rounds_completes_the_session_and_totals_the_points()
    {
        var session = NewSession();

        for (var i = 0; i < GameRules.RoundsPerDay; i++)
        {
            session.RegisterPhase1Attempt(i, Guid.NewGuid(), correct: true);
            session.RegisterPhase2(i, Guid.NewGuid(), score: 100, now: DateTimeOffset.UnixEpoch.AddMinutes(i));
        }

        session.IsCompleted.ShouldBeTrue();
        session.TotalPoints.ShouldBe(500);
    }

    [Fact]
    public void Skipping_phase2_ends_the_round_with_no_phase2_points()
    {
        var session = NewSession();
        session.RegisterPhase1Attempt(0, Guid.NewGuid(), correct: true);

        session.SkipPhase2(0, DateTimeOffset.UnixEpoch);

        var round = session.Rounds[0];
        round.Phase2Done.ShouldBeTrue();
        round.Phase2Skipped.ShouldBeTrue();
        round.Phase2Points.ShouldBe(0);
        round.Points.ShouldBe(50);
    }

    [Fact]
    public void Phase2_cannot_be_skipped_before_phase1_is_done_or_after_it_was_answered()
    {
        var session = NewSession();
        Should.Throw<GameRuleException>(() => session.SkipPhase2(0, DateTimeOffset.UnixEpoch));

        session.RegisterPhase1Attempt(0, Guid.NewGuid(), correct: true);
        session.RegisterPhase2(0, Guid.NewGuid(), 70, DateTimeOffset.UnixEpoch, matchStartSeconds: 4.2);
        session.Rounds[0].Phase2MatchStartSeconds.ShouldBe(4.2);
        Should.Throw<GameRuleException>(() => session.SkipPhase2(0, DateTimeOffset.UnixEpoch));
    }

    [Fact]
    public void A_day_of_skipped_phase2_rounds_still_completes_the_session()
    {
        var session = NewSession();

        for (var i = 0; i < GameRules.RoundsPerDay; i++)
        {
            session.RegisterPhase1Attempt(i, Guid.NewGuid(), correct: true);
            session.SkipPhase2(i, DateTimeOffset.UnixEpoch);
        }

        session.IsCompleted.ShouldBeTrue();
        session.TotalPoints.ShouldBe(250);
    }

    [Fact]
    public void Phase2_cannot_be_answered_twice()
    {
        var session = NewSession();
        session.RegisterPhase1Attempt(0, Guid.NewGuid(), correct: true);
        session.RegisterPhase2(0, Guid.NewGuid(), 40, DateTimeOffset.UnixEpoch);

        Should.Throw<GameRuleException>(() => session.RegisterPhase2(0, Guid.NewGuid(), 90, DateTimeOffset.UnixEpoch));
    }
}
