namespace SongTheSame.Audio;

/// <param name="Cost">Average cosine distance per query frame along the best alignment (0 = identical).</param>
/// <param name="Transposition">Semitone shift of the query that matched best (0-11).</param>
/// <param name="EndFrame">Frame in the reference where the best alignment ends.</param>
public readonly record struct SimilarityResult(float Cost, int Transposition, int EndFrame);

/// <summary>
/// Finds the part of <c>reference</c> that sounds most like <c>query</c>, tolerant to tempo changes
/// (via DTW) and to transposition (by trying all 12 circular pitch-class rotations).
/// </summary>
public static class ChromaSimilarityScorer
{
    private const int Dim = ChromaSequence.PitchClasses;

    public static SimilarityResult Compare(ChromaSequence query, ChromaSequence reference)
    {
        var best = new SimilarityResult(float.PositiveInfinity, 0, 0);
        var cost = new float[query.FrameCount * reference.FrameCount];

        for (var shift = 0; shift < Dim; shift++)
        {
            FillCostMatrix(query, reference, shift, cost);
            var match = SubsequenceDtw.Align(cost, query.FrameCount, reference.FrameCount);
            if (match.AverageCost < best.Cost)
                best = new SimilarityResult(match.AverageCost, shift, match.EndFrame);
        }

        return best;
    }

    private static void FillCostMatrix(ChromaSequence query, ChromaSequence reference, int shift, float[] cost)
    {
        var n = reference.FrameCount;
        for (var i = 0; i < query.FrameCount; i++)
        {
            var q = query.Frame(i);
            for (var j = 0; j < n; j++)
                cost[i * n + j] = 1f - RotatedDot(q, reference.Frame(j), shift);
        }
    }

    private static float RotatedDot(ReadOnlySpan<float> a, ReadOnlySpan<float> b, int shift)
    {
        var sum = 0f;
        for (var k = 0; k < Dim; k++)
            sum += a[(k + shift) % Dim] * b[k];
        return sum;
    }
}
