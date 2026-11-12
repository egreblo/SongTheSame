using SongTheSame.Application.Abstractions;

namespace SongTheSame.Application.Features.Admin.ReviewReport;

public sealed record ReviewReportCommand(Guid ReportId, string? Note);

public sealed class ReviewReportHandler(IReportRepository reports, TimeProvider clock) : IHandler<ReviewReportCommand, bool>
{
    public async Task<bool> HandleAsync(ReviewReportCommand request, CancellationToken cancellationToken)
    {
        var report = await reports.GetAsync(request.ReportId, cancellationToken)
            ?? throw new KeyNotFoundException("Report not found.");

        report.MarkReviewed(request.Note, clock.GetUtcNow());
        await reports.SaveAsync(cancellationToken);
        return true;
    }
}
