using SongTheSame.Domain.Similarity;

namespace SongTheSame.Application.Abstractions;

public interface ICalibrationStore
{
    Task<SimilarityCalibration?> GetLatestAsync(CancellationToken cancellationToken);

    Task SaveAsync(SimilarityCalibration calibration, CancellationToken cancellationToken);
}
