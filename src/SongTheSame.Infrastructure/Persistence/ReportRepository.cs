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

    public async Task<IReadOnlyList<RoundReport>> ListAsync(ReportStatus status, int take, CancellationToken cancellationToken) =>
        await db.RoundReports.AsNoTracking()
            .Where(report => report.Status == status)
            .OrderByDescending(report => report.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<RoundReport?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        db.RoundReports.FirstOrDefaultAsync(report => report.Id == id, cancellationToken);

    public Task SaveAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
