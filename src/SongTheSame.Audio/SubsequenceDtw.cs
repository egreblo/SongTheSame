namespace SongTheSame.Audio;

/// <param name="AverageCost">Cost per query frame along the best alignment (0 = identical).</param>
/// <param name="StartFrame">Reference frame where the best alignment begins.</param>
/// <param name="EndFrame">Reference frame where the best alignment ends.</param>
public readonly record struct DtwMatch(float AverageCost, int StartFrame, int EndFrame);

/// <summary>
/// Subsequence DTW: finds the best alignment of a whole query inside a longer reference.
/// Steps (1,1), (1,2), (2,1) limit tempo deviation to a factor of two, and costs are weighted
/// so every query frame is counted exactly once, making AverageCost comparable across paths.
/// The start of the alignment is carried along the path so callers know where the match begins.
/// </summary>
public static class SubsequenceDtw
{
    public static DtwMatch Align(ReadOnlySpan<float> cost, int queryFrames, int referenceFrames)
    {
        var m = queryFrames;
        var n = referenceFrames;
        var accumulated = new float[m * n];
        var starts = new int[m * n];

        for (var j = 0; j < n; j++)
        {
            accumulated[j] = cost[j];
            starts[j] = j;
        }

        for (var i = 1; i < m; i++)
        {
            for (var j = 0; j < n; j++)
                (accumulated[i * n + j], starts[i * n + j]) = BestStep(cost, accumulated, starts, i, j, n);
        }

        return BestEnd(accumulated, starts, m, n);
    }

    private static (float Cost, int Start) BestStep(ReadOnlySpan<float> cost, float[] accumulated, int[] starts, int i, int j, int n)
    {
        var current = cost[i * n + j];
        var best = (Cost: float.PositiveInfinity, Start: 0);

        if (j >= 1)
            best = Better(best, accumulated[(i - 1) * n + j - 1] + current, starts[(i - 1) * n + j - 1]);

        if (j >= 2)
        {
            var pairCost = (current + cost[i * n + j - 1]) * 0.5f;
            best = Better(best, accumulated[(i - 1) * n + j - 2] + pairCost, starts[(i - 1) * n + j - 2]);
        }

        if (i == 1)
            best = Better(best, current + cost[(i - 1) * n + j], j);
        else if (j >= 1)
            best = Better(best, accumulated[(i - 2) * n + j - 1] + current + cost[(i - 1) * n + j], starts[(i - 2) * n + j - 1]);

        return best;
    }

    private static (float Cost, int Start) Better((float Cost, int Start) best, float cost, int start) =>
        cost < best.Cost ? (cost, start) : best;

    private static DtwMatch BestEnd(float[] accumulated, int[] starts, int m, int n)
    {
        var bestCost = float.PositiveInfinity;
        var bestEnd = 0;
        for (var j = 0; j < n; j++)
        {
            var value = accumulated[(m - 1) * n + j];
            if (value >= bestCost) continue;
            bestCost = value;
            bestEnd = j;
        }

        return new DtwMatch(bestCost / m, starts[(m - 1) * n + bestEnd], bestEnd);
    }
}
