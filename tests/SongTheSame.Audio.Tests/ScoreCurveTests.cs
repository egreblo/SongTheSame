using SongTheSame.Audio;

namespace SongTheSame.Audio.Tests;

public class ScoreCurveTests
{
    private static ScoreCurve CurveWithCostsFromZeroToOne() =>
        new(Enumerable.Range(0, 1000).Select(i => i / 1000f));

    [Fact]
    public void A_cost_better_than_every_random_pair_scores_100()
    {
        CurveWithCostsFromZeroToOne().Score(-0.5f).ShouldBe(100);
    }

    [Fact]
    public void A_typical_random_cost_scores_the_minimum()
    {
        var curve = CurveWithCostsFromZeroToOne();

        curve.Score(0.5f).ShouldBe(1);
        curve.Score(0.9f).ShouldBe(1);
    }

    [Fact]
    public void Score_falls_as_cost_rises()
    {
        var curve = CurveWithCostsFromZeroToOne();

        var scores = new[] { 0.01f, 0.05f, 0.1f, 0.2f, 0.3f }.Select(curve.Score).ToArray();

        scores.ShouldBe(scores.OrderByDescending(s => s).ToArray());
        scores.Distinct().Count().ShouldBe(scores.Length);
    }

    [Fact]
    public void Top_percent_of_random_pairs_scores_high()
    {
        // 1% of random pairs are better than 0.01 -> p = 0.01, score = 100 * (1 - 0.01 / 0.35) = 97
        CurveWithCostsFromZeroToOne().Score(0.01f).ShouldBe(97);
    }

    [Fact]
    public void Needs_at_least_two_samples()
    {
        Should.Throw<ArgumentException>(() => new ScoreCurve([0.1f]));
    }
}
