namespace SongTheSame.Audio;

public readonly record struct DtwMatch(float AverageCost, int EndFrame);

/// <summary>
/// Subsequence DTW: finds the best alignment of a whole query inside a longer reference.
/// Steps (1,1), (1,2), (2,1) limit tempo deviation to a factor of two, and costs are weighted
/// so every query frame is counted exactly once, making AverageCost comparable across paths.
/// </summary>
public static class SubsequenceDtw
{
    public static DtwMatch Align(ReadOnlySpan<float> cost, int queryFrames, int referenceFrames)
    {
        var m = queryFrames;
        var n = referenceFrames;
        var acc = new float[m * n];

        for (var j = 0; j < n; j++) acc[j] = cost[j];

        for (var i = 1; i < m; i++)
        {
            for (var j = 0; j < n; j++)
                acc[i * n + j] = BestStep(cost, acc, i, j, n);
        }

        return BestEnd(acc, m, n);
    }

    private static float BestStep(ReadOnlySpan<float> cost, float[] acc, int i, int j, int n)
    {
        var current = cost[i * n + j];
        var best = float.PositiveInfinity;

        if (j >= 1)
            best = Math.Min(best, acc[(i - 1) * n + j - 1] + current);

        if (j >= 2)
        {
            var pairCost = (current + cost[i * n + j - 1]) * 0.5f;
            best = Math.Min(best, acc[(i - 1) * n + j - 2] + pairCost);
        }

        if (i == 1)
            best = Math.Min(best, current + cost[(i - 1) * n + j]);
        else if (j >= 1)
            best = Math.Min(best, acc[(i - 2) * n + j - 1] + current + cost[(i - 1) * n + j]);

        return best;
    }

    private static DtwMatch BestEnd(float[] acc, int m, int n)
    {
        var bestCost = float.PositiveInfinity;
        var bestEnd = 0;
        for (var j = 0; j < n; j++)
        {
            var value = acc[(m - 1) * n + j];
            if (value >= bestCost) continue;
            bestCost = value;
            bestEnd = j;
        }

        return new DtwMatch(bestCost / m, bestEnd);
    }
}
