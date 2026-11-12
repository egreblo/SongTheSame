using SongTheSame.Domain.Game;

namespace SongTheSame.Domain.Tests.Game;

public class RoundScoringTests
{
    [Theory]
    [InlineData(1, 50)]
    [InlineData(2, 42)]
    [InlineData(3, 34)]
    [InlineData(6, 10)]
    public void Phase1_points_drop_with_every_extra_attempt(int attempts, int expected)
    {
        RoundScoring.Phase1Points(attempts, solved: true).ShouldBe(expected);
    }

    [Fact]
    public void Unsolved_phase1_gives_no_points()
    {
        RoundScoring.Phase1Points(6, solved: false).ShouldBe(0);
    }

    [Theory]
    [InlineData(100, 50)]
    [InlineData(1, 1)]
    [InlineData(57, 29)]
    public void Phase2_points_are_half_the_similarity_score(int score, int expected)
    {
        RoundScoring.Phase2Points(score).ShouldBe(expected);
    }

    [Theory]
    [InlineData(80, 80, 100)]
    [InlineData(40, 80, 50)]
    [InlineData(90, 80, 100)]
    [InlineData(1, 80, 1)]
    [InlineData(30, 0, 30)]
    public void Scores_are_scaled_so_the_best_available_match_is_100(int raw, int best, int expected)
    {
        RoundScoring.RelativeToBest(raw, best).ShouldBe(expected);
    }

    [Fact]
    public void A_perfect_day_is_500_points()
    {
        GameRules.MaxDailyPoints.ShouldBe(500);
    }
}
