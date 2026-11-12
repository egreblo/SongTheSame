using SongTheSame.Api.Auth;
using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Admin;
using SongTheSame.Application.Features.Admin.ListReports;
using SongTheSame.Application.Features.Admin.ReviewReport;
using SongTheSame.Domain.Reports;

namespace SongTheSame.Api.Endpoints;

public sealed record ReviewReportRequest(string? Note);

/// <summary>Admin-only: review the scores players flagged as wrong.</summary>
public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin").WithTags("Admin").RequireAuthorization(AdminOptions.PolicyName);

        group.MapGet("/reports", async (ReportStatus? status, IHandler<ListReportsQuery, IReadOnlyList<ReportItem>> handler, CancellationToken ct) =>
                await handler.HandleAsync(new ListReportsQuery(status ?? ReportStatus.Open), ct))
            .WithName("ListReports");

        group.MapPost("/reports/{reportId:guid}/review", async (
                Guid reportId, ReviewReportRequest body, IHandler<ReviewReportCommand, bool> handler, CancellationToken ct) =>
                await handler.HandleAsync(new ReviewReportCommand(reportId, body.Note), ct))
            .WithName("ReviewReport");

        return group;
    }
}
