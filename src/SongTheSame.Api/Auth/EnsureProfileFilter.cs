using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Players;
using SongTheSame.Application.Features.Players.GetMe;

namespace SongTheSame.Api.Auth;

/// <summary>
/// Makes sure a signed-in player has a profile (and so a display name) before they join a league or create a game,
/// even if they never opened their account page.
/// </summary>
public sealed class EnsureProfileFilter(IHandler<GetMeQuery, ProfileView> getMe) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var user = context.HttpContext.User;
        if (AuthenticatedUser.GetId(user) is { } userId)
            await getMe.HandleAsync(new GetMeQuery(userId, AuthenticatedUser.SuggestedName(user)), context.HttpContext.RequestAborted);

        return await next(context);
    }
}
