using SongTheSame.Api.Auth;
using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Leagues;
using SongTheSame.Application.Features.Leagues.CreateLeague;
using SongTheSame.Application.Features.Leagues.GetLeagueStandings;
using SongTheSame.Application.Features.Leagues.JoinLeague;
using SongTheSame.Application.Features.Leagues.LeaveLeague;
using SongTheSame.Application.Features.Leagues.ListMyLeagues;

namespace SongTheSame.Api.Endpoints;

public sealed record CreateLeagueRequest(string Name);

public sealed record JoinLeagueRequest(string InviteCode);

/// <summary>Leagues of friends; all endpoints need a signed-in user.</summary>
public static class LeagueEndpoints
{
    public static IEndpointRouteBuilder MapLeagueEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/leagues").WithTags("Leagues").RequireAuthorization().AddEndpointFilter<EnsureProfileFilter>();

        group.MapGet("/", async (HttpContext http, IHandler<ListMyLeaguesQuery, IReadOnlyList<LeagueSummary>> handler, CancellationToken ct) =>
                await handler.HandleAsync(new ListMyLeaguesQuery(UserId(http)), ct))
            .WithName("ListMyLeagues");

        group.MapPost("/", async (CreateLeagueRequest body, HttpContext http, IHandler<CreateLeagueCommand, LeagueSummary> handler, CancellationToken ct) =>
                await handler.HandleAsync(new CreateLeagueCommand(UserId(http), body.Name), ct))
            .WithName("CreateLeague");

        group.MapPost("/join", async (JoinLeagueRequest body, HttpContext http, IHandler<JoinLeagueCommand, LeagueSummary> handler, CancellationToken ct) =>
                await handler.HandleAsync(new JoinLeagueCommand(UserId(http), body.InviteCode), ct))
            .WithName("JoinLeague");

        group.MapGet("/{leagueId:guid}/standings", async (Guid leagueId, HttpContext http, IHandler<GetLeagueStandingsQuery, LeagueStandings> handler, CancellationToken ct) =>
                await handler.HandleAsync(new GetLeagueStandingsQuery(UserId(http), leagueId), ct))
            .WithName("GetLeagueStandings");

        group.MapPost("/{leagueId:guid}/leave", async (Guid leagueId, HttpContext http, IHandler<LeaveLeagueCommand, bool> handler, CancellationToken ct) =>
                await handler.HandleAsync(new LeaveLeagueCommand(UserId(http), leagueId), ct))
            .WithName("LeaveLeague");

        return group;
    }

    private static Guid UserId(HttpContext http) =>
        AuthenticatedUser.GetId(http.User) ?? throw new UnauthorizedAccessException();
}
