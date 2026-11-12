using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Catalog.IngestPlaylist;
using SongTheSame.Application.Features.Catalog.IngestTrack;
using SongTheSame.Application.Tests.Fakes;
using SongTheSame.Domain.Songs;

namespace SongTheSame.Application.Tests.Features.Catalog;

public class IngestPlaylistHandlerTests
{
    private static SourceTrack Track(long id, string? preview) =>
        new(id, $"Song {id}", "Artist", null, null, 180, 1000, preview);

    [Fact]
    public async Task Counts_added_skipped_and_failed_tracks_and_keeps_going_after_a_failure()
    {
        var source = new FakeMusicSource(
            Track(1, "https://cdn/1.mp3"),
            Track(2, "https://cdn/broken.mp3"),
            Track(3, null),
            Track(4, "https://cdn/4.mp3"));
        var catalog = new InMemorySongCatalog();
        var ingestTrack = new IngestTrackHandler(catalog, source, new FakeChromaExtractor(), new FakeTimeProvider());
        var handler = new IngestPlaylistHandler(source, ingestTrack, NullLogger<IngestPlaylistHandler>.Instance);

        var result = await handler.HandleAsync(
            new IngestPlaylistCommand(99, SongOrigin.International, Limit: 10), CancellationToken.None);

        result.ShouldBe(new IngestPlaylistResult(Added: 2, Skipped: 1, Failed: 1));
        catalog.Songs.Select(song => song.DeezerId).ShouldBe([1L, 4L]);
    }

    [Fact]
    public async Task Respects_the_limit()
    {
        var source = new FakeMusicSource(Track(1, "https://cdn/1.mp3"), Track(2, "https://cdn/2.mp3"));
        var catalog = new InMemorySongCatalog();
        var ingestTrack = new IngestTrackHandler(catalog, source, new FakeChromaExtractor(), new FakeTimeProvider());
        var handler = new IngestPlaylistHandler(source, ingestTrack, NullLogger<IngestPlaylistHandler>.Instance);

        await handler.HandleAsync(new IngestPlaylistCommand(99, SongOrigin.Regional, Limit: 1), CancellationToken.None);

        catalog.Songs.Count.ShouldBe(1);
    }
}
