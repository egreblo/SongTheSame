namespace SongTheSame.Domain.Similarity;

/// <summary>
/// Costs measured between random song pairs. The newest calibration defines how raw alignment
/// costs are converted to 1-100 scores.
/// </summary>
public sealed class SimilarityCalibration
{
    private SimilarityCalibration()
    {
    }

    public Guid Id { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public int ExtractorVersion { get; private set; }

    public float[] SortedCosts { get; private set; } = [];

    public static SimilarityCalibration Create(int extractorVersion, IEnumerable<float> costs, DateTimeOffset now) =>
        new()
        {
            Id = Guid.NewGuid(),
            CreatedAt = now,
            ExtractorVersion = extractorVersion,
            SortedCosts = costs.OrderBy(cost => cost).ToArray(),
        };
}
