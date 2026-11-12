using SongTheSame.Domain.Players;

namespace SongTheSame.Domain.Tests.Players;

public class PlayerKeyTests
{
    [Fact]
    public void User_keys_round_trip_and_anonymous_keys_are_not_users()
    {
        var id = Guid.NewGuid();

        PlayerKey.TryGetUserId(PlayerKey.ForUser(id), out var parsed).ShouldBeTrue();
        parsed.ShouldBe(id);
        PlayerKey.TryGetUserId(PlayerKey.Anonymous(id), out _).ShouldBeFalse();
    }

    [Fact]
    public void Profile_names_are_trimmed_and_required()
    {
        var profile = Profile.Create(Guid.NewGuid(), "  Ana  ", DateTimeOffset.UnixEpoch);

        profile.DisplayName.ShouldBe("Ana");
        Should.Throw<ArgumentException>(() => profile.Rename("   "));
    }
}
