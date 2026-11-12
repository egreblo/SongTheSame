using SongTheSame.Application.Abstractions;

namespace SongTheSame.Application.Features.Players.UpdateProfile;

public sealed record UpdateProfileCommand(Guid UserId, string DisplayName);

public sealed class UpdateProfileHandler(IProfileRepository profiles) : IHandler<UpdateProfileCommand, ProfileView>
{
    public async Task<ProfileView> HandleAsync(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await profiles.GetAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Profile not found.");

        profile.Rename(request.DisplayName);
        await profiles.SaveAsync(cancellationToken);
        return new ProfileView(profile.UserId, profile.DisplayName);
    }
}
