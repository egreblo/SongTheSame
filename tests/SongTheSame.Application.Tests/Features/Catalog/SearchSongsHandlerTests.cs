using SongTheSame.Application.Features.Catalog.SearchSongs;
using SongTheSame.Application.Tests.Fakes;
using SongTheSame.Domain.Songs;

namespace SongTheSame.Application.Tests.Features.Catalog;

public class SearchSongsHandlerTests
{
    private static async Task<InMemorySongCatalog> CatalogWithSongsAsync()
    {
        var catalog = new InMemorySongCatalog();
        (string Title, string Artist, int Popularity)[] songs =
        [
            ("Moja Štikla", "Severina", 500),
            ("Bohemian Rhapsody", "Queen", 900),
            ("Under Pressure", "Queen", 800),
        ];

        long id = 1;
        foreach (var (title, artist, popularity) in songs)
        {
            var song = Song.Create(id++, title, artist, null, null, 200, popularity, SongOrigin.International, DateTimeOffset.UnixEpoch);
            await catalog.AddAsync(song, SongFeatures.Create(song.Id, 1, 10, 300, []), CancellationToken.None);
        }

        return catalog;
    }

    [Fact]
    public async Task Finds_songs_ignoring_accents_and_case_ordered_by_popularity()
    {
        var handler = new SearchSongsHandler(await CatalogWithSongsAsync());

        var result = await handler.HandleAsync(new SearchSongsQuery("QUEEN"), CancellationToken.None);

        result.Select(song => song.Title).ShouldBe(["Bohemian Rhapsody", "Under Pressure"]);
    }

    [Fact]
    public async Task Matches_all_words_in_any_order()
    {
        var handler = new SearchSongsHandler(await CatalogWithSongsAsync());

        var result = await handler.HandleAsync(new SearchSongsQuery("severina stikla"), CancellationToken.None);

        result.ShouldHaveSingleItem().Title.ShouldBe("Moja Štikla");
    }

    [Fact]
    public async Task Returns_nothing_for_a_query_that_is_too_short()
    {
        var handler = new SearchSongsHandler(await CatalogWithSongsAsync());

        var result = await handler.HandleAsync(new SearchSongsQuery("q"), CancellationToken.None);

        result.ShouldBeEmpty();
    }
}
