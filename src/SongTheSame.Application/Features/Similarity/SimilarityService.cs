using SongTheSame.Application.Abstractions;
using SongTheSame.Audio;
using SongTheSame.Domain.Game;

namespace SongTheSame.Application.Features.Similarity;

public sealed class SimilarityService(ISongFeatureStore features, ICalibrationStore calibrations) : ISimilarityService
{
    /// <summary>Stage 1 compares at half the frame rate (about 4x cheaper); only the best of it is re-scored in full.</summary>
    private const int CoarseFactor = 2;

    private const int FinalistsPerResult = 8;
    private const int MinimumFinalists = 30;

    public async Task<SimilarityScore> ScoreAsync(
        Guid targetSongId, int segmentStartFrame, Guid guessSongId, CancellationToken cancellationToken)
    {
        var segment = await LoadSegmentAsync(targetSongId, segmentStartFrame, cancellationToken);
        var guess = await features.GetAsync(guessSongId, cancellationToken)
            ?? throw new KeyNotFoundException($"Song {guessSongId} has no features.");
        var curve = await LoadCurveAsync(cancellationToken);

        var cost = ChromaSimilarityScorer.Compare(segment, guess).Cost;
        return new SimilarityScore(curve.Score(cost), cost);
    }

    public async Task<IReadOnlyList<SimilarMatch>> FindBestMatchesAsync(
        Guid targetSongId, int segmentStartFrame, int count, CancellationToken cancellationToken)
    {
        var segment = await LoadSegmentAsync(targetSongId, segmentStartFrame, cancellationToken);
        var curve = await LoadCurveAsync(cancellationToken);
        var candidates = (await features.GetAllAsync(cancellationToken)).Where(song => song.SongId != targetSongId).ToList();

        var finalists = Shortlist(segment, candidates, Math.Max(MinimumFinalists, count * FinalistsPerResult), cancellationToken);
        var costs = new (Guid SongId, float Cost)[finalists.Count];
        Parallel.For(0, finalists.Count, new ParallelOptions { CancellationToken = cancellationToken }, index =>
            costs[index] = (finalists[index].SongId, ChromaSimilarityScorer.Compare(segment, finalists[index].Chroma).Cost));

        return costs
            .OrderBy(entry => entry.Cost)
            .Take(count)
            .Select(entry => new SimilarMatch(entry.SongId, curve.Score(entry.Cost)))
            .ToList();
    }

    private static List<SongChroma> Shortlist(
        ChromaSequence segment, List<SongChroma> candidates, int size, CancellationToken cancellationToken)
    {
        if (candidates.Count <= size) return candidates;

        var coarseSegment = segment.Downsample(CoarseFactor);
        var coarseCosts = new float[candidates.Count];
        Parallel.For(0, candidates.Count, new ParallelOptions { CancellationToken = cancellationToken }, index =>
            coarseCosts[index] = ChromaSimilarityScorer.Compare(coarseSegment, candidates[index].Chroma.Downsample(CoarseFactor)).Cost);

        return Enumerable.Range(0, candidates.Count)
            .OrderBy(index => coarseCosts[index])
            .Take(size)
            .Select(index => candidates[index])
            .ToList();
    }

    private async Task<ChromaSequence> LoadSegmentAsync(Guid songId, int startFrame, CancellationToken cancellationToken)
    {
        var chroma = await features.GetAsync(songId, cancellationToken)
            ?? throw new KeyNotFoundException($"Song {songId} has no features.");
        return chroma.Slice(startFrame, GameRules.SegmentFrames);
    }

    private async Task<ScoreCurve> LoadCurveAsync(CancellationToken cancellationToken)
    {
        var calibration = await calibrations.GetLatestAsync(cancellationToken)
            ?? throw new InvalidOperationException("No similarity calibration exists. Run `calibrate` in the Ingest tool.");
        return new ScoreCurve(calibration.SortedCosts);
    }
}
