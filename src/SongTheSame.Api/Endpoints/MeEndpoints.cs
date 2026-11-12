using SongTheSame.Api.Auth;
using SongTheSame.Api.Setup;
using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Players;
using SongTheSame.Application.Features.Players.ClaimSessions;
using SongTheSame.Application.Features.Players.GetMe;
using SongTheSame.Application.Features.Players.UpdateProfile;

namespace SongTheSame.Api.Endpoints;

public sealed record UpdateProfileRequest(string DisplayName);

public sealed record ClaimSessionsResponse(int Moved);

public sealed record MeResponse(Guid UserId, string DisplayName, bool IsAdmin);

/// <summary>Account endpoints; all require a valid Supabase token.</summary>
public static class MeEndpoints
{
    public static IEndpointRouteBuilder MapMeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/me").WithTags("Me").RequireAuthorization();

        group.MapGet("/", async (HttpContext http, AdminOptions admin, IHandler<GetMeQuery, ProfileView> handler, CancellationToken ct) =>
            {
                var profile = await handler.HandleAsync(new GetMeQuery(UserId(http), AuthenticatedUser.SuggestedName(http.User)), ct);
                return new MeResponse(profile.UserId, profile.DisplayName, admin.IsAdmin(http.User));
            })
            .WithName("GetMe");

        group.MapPut("/", async (UpdateProfileRequest body, HttpContext http, IHandler<UpdateProfileCommand, ProfileView> handler, CancellationToken ct) =>
                await handler.HandleAsync(new UpdateProfileCommand(UserId(http), body.DisplayName), ct))
            .WithName("UpdateMe");

        group.MapPost("/claim-sessions", async (HttpContext http, IHandler<ClaimSessionsCommand, int> handler, CancellationToken ct) =>
            {
                var anonymous = PlayerIdentity.AnonymousFromRequest(http);
                if (anonymous is null) return Results.Ok(new ClaimSessionsResponse(0));
                return Results.Ok(new ClaimSessionsResponse(await handler.HandleAsync(new ClaimSessionsCommand(UserId(http), anonymous), ct)));
            })
            .WithName("ClaimSessions")
            .Produces<ClaimSessionsResponse>();

        return group;
    }

    private static Guid UserId(HttpContext http) =>
        AuthenticatedUser.GetId(http.User) ?? throw new UnauthorizedAccessException();
}
