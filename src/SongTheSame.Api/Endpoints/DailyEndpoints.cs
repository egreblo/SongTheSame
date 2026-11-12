using SongTheSame.Api.Setup;
using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Game.Common;
using SongTheSame.Application.Features.Game.GetDailyStats;
using SongTheSame.Application.Features.Game.GetSession;
using SongTheSame.Application.Features.Game.GuessSong;
using SongTheSame.Application.Features.Game.ReportRound;
using SongTheSame.Application.Features.Game.SkipClip;
using SongTheSame.Application.Features.Game.StartSession;
using SongTheSame.Application.Features.Game.SubmitSimilarity;

namespace SongTheSame.Api.Endpoints;

public sealed record SongGuessRequest(Guid SongId);

public sealed record ReportRequest(string? Note);

/// <summary>The daily game: every call returns the full <see cref="SessionView"/> so clients just replace their state.</summary>
public static class DailyEndpoints
{
    public static IEndpointRouteBuilder MapDailyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/daily").WithTags("Daily");

        group.MapPost("/session", (
                HttpContext http, IHandler<StartSessionCommand, SessionView> handler, CancellationToken ct) =>
                WithPlayer(http, player => handler.HandleAsync(new StartSessionCommand(player), ct)))
            .WithName("StartDailySession")
            .Produces<SessionView>();

        group.MapGet("/session/{sessionId:guid}", (
                Guid sessionId, HttpContext http, IHandler<GetSessionQuery, SessionView> handler, CancellationToken ct) =>
                WithPlayer(http, player => handler.HandleAsync(new GetSessionQuery(sessionId, player), ct)))
            .WithName("GetDailySession")
            .Produces<SessionView>();

        group.MapPost("/session/{sessionId:guid}/rounds/{round:int}/guess", (
                Guid sessionId, int round, SongGuessRequest body, HttpContext http,
                IHandler<GuessSongCommand, SessionView> handler, CancellationToken ct) =>
                WithPlayer(http, player => handler.HandleAsync(new GuessSongCommand(sessionId, player, round, body.SongId), ct)))
            .WithName("GuessSong")
            .Produces<SessionView>();

        group.MapPost("/session/{sessionId:guid}/rounds/{round:int}/skip", (
                Guid sessionId, int round, HttpContext http,
                IHandler<SkipClipCommand, SessionView> handler, CancellationToken ct) =>
                WithPlayer(http, player => handler.HandleAsync(new SkipClipCommand(sessionId, player, round), ct)))
            .WithName("SkipClip")
            .Produces<SessionView>();

        group.MapPost("/session/{sessionId:guid}/rounds/{round:int}/similarity", (
                Guid sessionId, int round, SongGuessRequest body, HttpContext http,
                IHandler<SubmitSimilarityCommand, SessionView> handler, CancellationToken ct) =>
                WithPlayer(http, player => handler.HandleAsync(new SubmitSimilarityCommand(sessionId, player, round, body.SongId), ct)))
            .WithName("SubmitSimilarity")
            .Produces<SessionView>();

        group.MapGet("/session/{sessionId:guid}/stats", (
                Guid sessionId, HttpContext http, IHandler<GetDailyStatsQuery, DailyStatsView> handler, CancellationToken ct) =>
                WithPlayer(http, player => handler.HandleAsync(new GetDailyStatsQuery(sessionId, player), ct)))
            .WithName("GetDailyStats")
            .Produces<DailyStatsView>();

        group.MapPost("/session/{sessionId:guid}/rounds/{round:int}/report", (
                Guid sessionId, int round, ReportRequest body, HttpContext http,
                IHandler<ReportRoundCommand, bool> handler, CancellationToken ct) =>
                WithPlayer(http, player => handler.HandleAsync(new ReportRoundCommand(sessionId, player, round, body.Note), ct)))
            .WithName("ReportRound")
            .Produces<bool>();

        return group;
    }

    private static async Task<IResult> WithPlayer<T>(HttpContext http, Func<string, Task<T>> action)
    {
        var player = PlayerIdentity.FromRequest(http);
        if (player is null)
            return Results.Problem($"Header {PlayerIdentity.AnonymousHeader} must contain a GUID.", statusCode: StatusCodes.Status400BadRequest);

        return Results.Ok(await action(player));
    }
}
