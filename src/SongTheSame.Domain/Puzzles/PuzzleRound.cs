namespace SongTheSame.Domain.Puzzles;

public sealed record RoundMatch(Guid SongId, int Score);

/// <summary>One song of a daily puzzle and the segment players must match in phase 2.</summary>
public sealed class PuzzleRound
{
    private PuzzleRound()
    {
    }

    public Guid Id { get; private set; }

    /// <summary>Zero-based position within the day.</summary>
    public int Index { get; private set; }

    public Guid SongId { get; private set; }

    public int SegmentStartFrame { get; private set; }

    public double SegmentStartSeconds { get; private set; }

    public double SegmentSeconds { get; private set; }

    /// <summary>The catalog songs that match the segment best, computed when the puzzle is generated.</summary>
    public List<RoundMatch> BestMatches { get; private set; } = [];

    public static PuzzleRound Create(
        int index, Guid songId, int segmentStartFrame, double segmentStartSeconds, double segmentSeconds, IEnumerable<RoundMatch> bestMatches) =>
        new()
        {
            Id = Guid.NewGuid(),
            Index = index,
            SongId = songId,
            SegmentStartFrame = segmentStartFrame,
            SegmentStartSeconds = segmentStartSeconds,
            SegmentSeconds = segmentSeconds,
            BestMatches = bestMatches.ToList(),
        };
}
