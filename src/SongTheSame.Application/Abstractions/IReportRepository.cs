using SongTheSame.Domain.Reports;

namespace SongTheSame.Application.Abstractions;

public interface IReportRepository
{
    /// <summary>Returns false when this round of this session was already reported.</summary>
    Task<bool> TryAddAsync(RoundReport report, CancellationToken cancellationToken);

    /// <summary>Reports with the given status, newest first (read-only).</summary>
    Task<IReadOnlyList<RoundReport>> ListAsync(ReportStatus status, int take, CancellationToken cancellationToken);

    /// <summary>A tracked report, for changing it.</summary>
    Task<RoundReport?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task SaveAsync(CancellationToken cancellationToken);
}
