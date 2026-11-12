using SongTheSame.Domain.Reports;

namespace SongTheSame.Application.Abstractions;

public interface IReportRepository
{
    /// <summary>Returns false when this round of this session was already reported.</summary>
    Task<bool> TryAddAsync(RoundReport report, CancellationToken cancellationToken);
}
