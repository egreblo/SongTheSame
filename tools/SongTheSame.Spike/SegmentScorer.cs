using SongTheSame.Audio;

namespace SongTheSame.Spike;

/// <summary>Scores a song as a query by trying several fixed-length segments and keeping the best.</summary>
public sealed class SegmentScorer
{
    public const int SegmentFrames = 60;
    private static readonly int[] SegmentStarts = [20, 70, 120, 170, 220];

    public float BestCost(ChromaSequence query, ChromaSequence reference)
    {
        var best = float.PositiveInfinity;
        foreach (var start in SegmentStarts)
        {
            if (start + SegmentFrames > query.FrameCount) continue;
            var segment = query.Slice(start, SegmentFrames);
            best = Math.Min(best, ChromaSimilarityScorer.Compare(segment, reference).Cost);
        }

        return best;
    }
}
