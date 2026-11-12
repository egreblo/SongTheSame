using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Similarity.CalibrateSimilarity;

namespace SongTheSame.Ingest.Commands;

public sealed class CalibrateCommand(IHandler<CalibrateSimilarityCommand, CalibrationResult> calibrate)
{
    private const int DefaultSamples = 3000;

    public async Task<int> RunAsync(string[] args)
    {
        var samples = args.Length > 0 && int.TryParse(args[0], out var parsed) ? parsed : DefaultSamples;

        var result = await calibrate.HandleAsync(new CalibrateSimilarityCommand(samples), CancellationToken.None);
        Console.WriteLine(
            $"Calibrated with {result.Samples} random pairs. Cost percentiles: 1% {result.Percentile1Cost:F3}, 10% {result.Percentile10Cost:F3}, median {result.MedianCost:F3}");
        return 0;
    }
}
