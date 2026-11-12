using Microsoft.EntityFrameworkCore;
using SongTheSame.Application.Abstractions;
using SongTheSame.Domain.Similarity;

namespace SongTheSame.Infrastructure.Persistence;

internal sealed class CalibrationStore(AppDbContext db) : ICalibrationStore
{
    public Task<SimilarityCalibration?> GetLatestAsync(CancellationToken cancellationToken) =>
        db.SimilarityCalibrations.AsNoTracking()
            .OrderByDescending(calibration => calibration.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task SaveAsync(SimilarityCalibration calibration, CancellationToken cancellationToken)
    {
        db.SimilarityCalibrations.Add(calibration);
        await db.SaveChangesAsync(cancellationToken);
    }
}
