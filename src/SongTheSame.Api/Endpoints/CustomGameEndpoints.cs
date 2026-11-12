using SongTheSame.Api.Auth;
using SongTheSame.Api.Setup;
using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.CustomGames;
using SongTheSame.Application.Features.CustomGames.CreateCustomGame;
using SongTheSame.Application.Features.CustomGames.GetCustomGame;
using SongTheSame.Application.Features.CustomGames.ListMyCustomGames;
using SongTheSame.Application.Features.CustomGames.StartCustomSession;
using SongTheSame.Application.Features.Game.Common;

namespace SongTheSame.Api.Endpoints;

public sealed record CreateCustomGameRequest(string Title, IReadOnlyList<Guid> SongIds);

/// <summary>Games created in the editor. Creating and listing need an account; playing does not.</summary>
public static class CustomGameEndpoints
{
    public static IEndpointRouteBuilder MapCustomGameEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/custom-games").WithTags("CustomGames");

        group.MapPost("/", async (CreateCustomGameRequest body, HttpContext http, IHandler<CreateCustomGameCommand, CustomGameSummary> handler, CancellationToken ct) =>
                await handler.HandleAsync(new CreateCustomGameCommand(UserId(http), body.Title, body.SongIds), ct))
            .RequireAuthorization()
            .AddEndpointFilter<EnsureProfileFilter>()
            .RequireRateLimiting(RateLimitSetup.ExpensivePolicy)
            .WithName("CreateCustomGame");

        group.MapGet("/mine", async (HttpContext http, IHandler<ListMyCustomGamesQuery, IReadOnlyList<CustomGameSummary>> handler, CancellationToken ct) =>
                await handler.HandleAsync(new ListMyCustomGamesQuery(UserId(http)), ct))
            .RequireAuthorization()
            .WithName("ListMyCustomGames");

        group.MapGet("/{code}", async (string code, IHandler<GetCustomGameQuery, CustomGameInfo> handler, CancellationToken ct) =>
                await handler.HandleAsync(new GetCustomGameQuery(code), ct))
            .WithName("GetCustomGame");

        group.MapPost("/{code}/session", async (string code, HttpContext http, IHandler<StartCustomSessionCommand, SessionView> handler, CancellationToken ct) =>
            {
                var player = PlayerIdentity.FromRequest(http);
                if (player is null)
                    return Results.Problem($"Header {PlayerIdentity.AnonymousHeader} must contain a GUID.", statusCode: StatusCodes.Status400BadRequest);
                return Results.Ok(await handler.HandleAsync(new StartCustomSessionCommand(player, code), ct));
            })
            .Produces<SessionView>()
            .WithName("StartCustomSession");

        return group;
    }

    private static Guid UserId(HttpContext http) =>
        AuthenticatedUser.GetId(http.User) ?? throw new UnauthorizedAccessException();
}
