namespace SongTheSame.Application.Features.Similarity.CalibrateSimilarity;

public sealed record CalibrateSimilarityCommand(int SampleCount = 3000, int Seed = 12345);

public sealed record CalibrationResult(int Samples, float MedianCost, float Percentile1Cost, float Percentile10Cost);
