using SongTheSame.Application.Abstractions;
using SongTheSame.Audio;
using SongTheSame.Domain.Game;
using SongTheSame.Domain.Similarity;

namespace SongTheSame.Application.Features.Similarity.CalibrateSimilarity;

/// <summary>
/// Samples random (segment, other song) pairs from the catalog, exactly the way the game compares them,
/// and stores the measured costs. Players' scores are later read off this distribution.
/// </summary>
public sealed class CalibrateSimilarityHandler(
    ISongFeatureStore features,
    ICalibrationStore calibrations,
    TimeProvider clock) : IHandler<CalibrateSimilarityCommand, CalibrationResult>
{
    private const int SegmentMargin = 10;

    public async Task<CalibrationResult> HandleAsync(CalibrateSimilarityCommand request, CancellationToken cancellationToken)
    {
        var catalog = await features.GetAllAsync(cancellationToken);
        if (catalog.Count < 10) throw new InvalidOperationException("Need at least 10 songs in the catalog to calibrate.");

        var costs = new float[request.SampleCount];
        Parallel.For(0, request.SampleCount, new ParallelOptions { CancellationToken = cancellationToken }, i =>
            costs[i] = SampleCost(catalog, new Random(request.Seed + i)));

        var calibration = SimilarityCalibration.Create(ChromaCodec.CurrentVersion, costs, clock.GetUtcNow());
        await calibrations.SaveAsync(calibration, cancellationToken);

        var sorted = calibration.SortedCosts;
        return new CalibrationResult(sorted.Length, sorted[sorted.Length / 2], sorted[sorted.Length / 100], sorted[sorted.Length / 10]);
    }

    private static float SampleCost(IReadOnlyList<SongChroma> catalog, Random random)
    {
        var target = catalog[random.Next(catalog.Count)];
        SongChroma other;
        do other = catalog[random.Next(catalog.Count)];
        while (other.SongId == target.SongId);

        var maxStart = target.Chroma.FrameCount - GameRules.SegmentFrames - SegmentMargin;
        var start = random.Next(SegmentMargin, Math.Max(SegmentMargin + 1, maxStart));
        var segment = target.Chroma.Slice(start, GameRules.SegmentFrames);
        return ChromaSimilarityScorer.Compare(segment, other.Chroma).Cost;
    }
}
