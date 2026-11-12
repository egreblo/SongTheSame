using Microsoft.EntityFrameworkCore;
using SongTheSame.Application.Abstractions;
using SongTheSame.Domain.Reports;

namespace SongTheSame.Infrastructure.Persistence;

internal sealed class ReportRepository(AppDbContext db) : IReportRepository
{
    public async Task<bool> TryAddAsync(RoundReport report, CancellationToken cancellationToken)
    {
        db.RoundReports.Add(report);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (UniqueViolation.IsUniqueViolation(exception))
        {
            db.ChangeTracker.Clear();
            return false;
        }
    }
}
