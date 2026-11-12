using SongTheSame.Domain.Songs;

namespace SongTheSame.Domain.Tests.Songs;

public class SongIdentityTests
{
    private static Song Make(long id, string title, string artist) =>
        Song.Create(id, title, artist, null, null, 200, 1, SongOrigin.International, DateTimeOffset.UnixEpoch);

    [Theory]
    [InlineData("Bohemian Rhapsody", "Bohemian Rhapsody (Remastered 2011)")]
    [InlineData("Hotel California", "Hotel California (2013 Remaster)")]
    [InlineData("I Will Survive", "I Will Survive - Original 7\" Version")]
    [InlineData("Under Pressure", "Under Pressure (feat. David Bowie)")]
    public void Editions_of_the_same_song_are_the_same_work(string original, string edition)
    {
        SongIdentity.IsSameWork(Make(1, original, "Queen"), Make(2, edition, "Queen")).ShouldBeTrue();
    }

    [Fact]
    public void Different_songs_or_artists_are_different_works()
    {
        SongIdentity.IsSameWork(Make(1, "Hurt", "Johnny Cash"), Make(2, "Hurt", "Nine Inch Nails")).ShouldBeFalse();
        SongIdentity.IsSameWork(Make(1, "Hurt", "Johnny Cash"), Make(2, "Hurt Me", "Johnny Cash")).ShouldBeFalse();
    }
}
