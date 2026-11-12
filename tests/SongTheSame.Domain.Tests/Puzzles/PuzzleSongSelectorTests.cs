using SongTheSame.Domain.Game;
using SongTheSame.Domain.Puzzles;
using SongTheSame.Domain.Songs;

namespace SongTheSame.Domain.Tests.Puzzles;

public class PuzzleSongSelectorTests
{
    private static List<Song> Catalog() =>
        Enumerable.Range(1, 60)
            .Select(i => Song.Create(i, $"Song {i}", $"Artist {i}", null, null, 200, 1000 - i,
                i % 4 == 0 ? SongOrigin.Regional : SongOrigin.International, DateTimeOffset.UnixEpoch))
            .ToList();

    [Fact]
    public void Picks_five_distinct_songs_with_at_least_one_regional()
    {
        var picked = PuzzleSongSelector.Select(new DateOnly(2026, 11, 13), Catalog(), new HashSet<Guid>());

        picked.Count.ShouldBe(GameRules.RoundsPerDay);
        picked.Select(song => song.Id).Distinct().Count().ShouldBe(5);
        picked.Count(song => song.Origin == SongOrigin.Regional).ShouldBeInRange(1, 2);
    }

    [Fact]
    public void Is_deterministic_for_a_day_and_differs_between_days()
    {
        var catalog = Catalog();

        var first = PuzzleSongSelector.Select(new DateOnly(2026, 11, 13), catalog, new HashSet<Guid>());
        var again = PuzzleSongSelector.Select(new DateOnly(2026, 11, 13), catalog, new HashSet<Guid>());
        var next = PuzzleSongSelector.Select(new DateOnly(2026, 11, 14), catalog, new HashSet<Guid>());

        again.Select(s => s.Id).ShouldBe(first.Select(s => s.Id));
        next.Select(s => s.Id).ShouldNotBe(first.Select(s => s.Id));
    }

    [Fact]
    public void Skips_recently_played_songs()
    {
        var catalog = Catalog();
        var recent = catalog.Take(30).Select(song => song.Id).ToHashSet();

        var picked = PuzzleSongSelector.Select(new DateOnly(2026, 11, 13), catalog, recent);

        picked.ShouldAllBe(song => !recent.Contains(song.Id));
    }

    [Fact]
    public void Orders_the_most_popular_song_first()
    {
        var picked = PuzzleSongSelector.Select(new DateOnly(2026, 11, 13), Catalog(), new HashSet<Guid>());

        picked.Select(s => s.Popularity).ShouldBe(picked.Select(s => s.Popularity).OrderByDescending(p => p));
    }

    [Fact]
    public void Throws_when_the_catalog_is_too_small()
    {
        var tiny = Catalog().Take(3).ToList();

        Should.Throw<InvalidOperationException>(() => PuzzleSongSelector.Select(new DateOnly(2026, 11, 13), tiny, new HashSet<Guid>()));
    }
}
