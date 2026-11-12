using Microsoft.EntityFrameworkCore;
using SongTheSame.Domain.Songs;
using SongTheSame.Infrastructure.Persistence;

namespace SongTheSame.Infrastructure.Tests.Persistence;

public class SongCatalogTests
{
    private static async Task AddAsync(SongCatalog catalog, long deezerId, string title, string artist, int popularity)
    {
        var song = Song.Create(deezerId, title, artist, null, null, 200, popularity, SongOrigin.International, DateTimeOffset.UtcNow);
        var features = SongFeatures.Create(song.Id, 1, 10, 300, new byte[3600]);
        await catalog.AddAsync(song, features, CancellationToken.None);
    }

    [RequiresDatabaseFact]
    public async Task Stores_songs_and_searches_by_all_tokens_ignoring_accents_ordered_by_popularity()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        await using var context = database.NewContext();
        var catalog = new SongCatalog(context);

        await AddAsync(catalog, 1, "Moja Štikla", "Severina", 500);
        await AddAsync(catalog, 2, "Bohemian Rhapsody", "Queen", 900);
        await AddAsync(catalog, 3, "Under Pressure", "Queen", 800);

        (await catalog.ExistsAsync(2, CancellationToken.None)).ShouldBeTrue();
        (await catalog.ExistsAsync(99, CancellationToken.None)).ShouldBeFalse();

        var queen = await catalog.SearchAsync(["queen"], 10, CancellationToken.None);
        queen.Select(song => song.Title).ShouldBe(["Bohemian Rhapsody", "Under Pressure"]);

        var accentless = await catalog.SearchAsync(["stikla", "sev"], 10, CancellationToken.None);
        accentless.ShouldHaveSingleItem().Title.ShouldBe("Moja Štikla");

        (await context.SongFeatures.CountAsync()).ShouldBe(3);
    }

    [RequiresDatabaseFact]
    public async Task Search_treats_percent_and_underscore_literally()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        await using var context = database.NewContext();
        var catalog = new SongCatalog(context);
        await AddAsync(catalog, 1, "Anything", "Someone", 1);

        (await catalog.SearchAsync(["%"], 10, CancellationToken.None)).ShouldBeEmpty();
        (await catalog.SearchAsync(["_"], 10, CancellationToken.None)).ShouldBeEmpty();
    }

    [RequiresDatabaseFact]
    public async Task Duplicate_deezer_ids_are_rejected_by_the_unique_index()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        await using var context = database.NewContext();
        var catalog = new SongCatalog(context);
        await AddAsync(catalog, 7, "Song", "Artist", 1);

        await Should.ThrowAsync<DbUpdateException>(() => AddAsync(catalog, 7, "Song again", "Artist", 1));
    }
}
