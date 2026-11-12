namespace SongTheSame.Audio;

/// <summary>
/// Converts a raw alignment cost into the 1-100 score players see. It is calibrated against costs measured
/// between random song pairs: a cost that is among the best few percent of random pairs scores high, and a
/// cost no better than a typical random pair scores 1.
/// </summary>
public sealed class ScoreCurve
{
    /// <summary>Costs at or above this percentile of random pairs score the minimum of 1.</summary>
    public const double ZeroScorePercentile = 0.35;

    private readonly float[] _sortedRandomCosts;

    public ScoreCurve(IEnumerable<float> randomPairCosts)
    {
        _sortedRandomCosts = randomPairCosts.OrderBy(cost => cost).ToArray();
        if (_sortedRandomCosts.Length < 2)
            throw new ArgumentException("At least two calibration samples are required.", nameof(randomPairCosts));
    }

    public IReadOnlyList<float> SortedRandomCosts => _sortedRandomCosts;

    /// <summary>Fraction (0-1) of random pairs that matched better (lower cost) than this cost.</summary>
    public double RandomPercentile(float cost)
    {
        var index = Array.BinarySearch(_sortedRandomCosts, cost);
        if (index < 0) index = ~index;
        return (double)index / _sortedRandomCosts.Length;
    }

    public int Score(float cost)
    {
        var raw = 100.0 * (1.0 - RandomPercentile(cost) / ZeroScorePercentile);
        return Math.Clamp((int)Math.Round(raw), 1, 100);
    }
}
