using SongTheSame.Application.Abstractions;
using SongTheSame.Domain.Players;

namespace SongTheSame.Application.Features.Players.GetMe;

/// <param name="SuggestedName">Name from the identity provider, used only when the profile is created.</param>
public sealed record GetMeQuery(Guid UserId, string SuggestedName);

/// <summary>Returns the signed-in player's profile, creating it on first sign-in.</summary>
public sealed class GetMeHandler(IProfileRepository profiles, TimeProvider clock) : IHandler<GetMeQuery, ProfileView>
{
    private const string FallbackName = "Player";

    public async Task<ProfileView> HandleAsync(GetMeQuery request, CancellationToken cancellationToken)
    {
        var profile = await profiles.GetAsync(request.UserId, cancellationToken);
        if (profile is null)
        {
            var name = string.IsNullOrWhiteSpace(request.SuggestedName) ? FallbackName : request.SuggestedName;
            profile = Profile.Create(request.UserId, name, clock.GetUtcNow());
            if (!await profiles.TryAddAsync(profile, cancellationToken))
                profile = await profiles.GetAsync(request.UserId, cancellationToken) ?? profile;
        }

        return new ProfileView(profile.UserId, profile.DisplayName);
    }
}
