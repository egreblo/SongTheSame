using Microsoft.Extensions.Time.Testing;
using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Catalog.IngestTrack;
using SongTheSame.Application.Tests.Fakes;
using SongTheSame.Domain.Songs;

namespace SongTheSame.Application.Tests.Features.Catalog;

public class IngestTrackHandlerTests
{
    private readonly InMemorySongCatalog _catalog = new();
    private readonly FakeMusicSource _source = new();

    private IngestTrackHandler CreateHandler(int frames = 295) =>
        new(_catalog, _source, new FakeChromaExtractor(frames), new FakeTimeProvider());

    private static SourceTrack Track(long id = 1, string? preview = "https://cdn/preview.mp3") =>
        new(id, "Moja Štikla", "Severina", "Album", "https://cdn/cover.jpg", 200, 500000, preview);

    [Fact]
    public async Task Stores_song_and_features_for_a_new_track()
    {
        var outcome = await CreateHandler().HandleAsync(
            new IngestTrackCommand(Track(), SongOrigin.Regional), CancellationToken.None);

        outcome.ShouldBe(IngestOutcome.Added);
        _catalog.Songs.ShouldHaveSingleItem().SearchText.ShouldBe("moja stikla severina");
        _catalog.Songs[0].Origin.ShouldBe(SongOrigin.Regional);
        var features = _catalog.Features.ShouldHaveSingleItem();
        features.SongId.ShouldBe(_catalog.Songs[0].Id);
        features.FrameCount.ShouldBe(295);
        features.Data.Length.ShouldBe(295 * 12);
    }

    [Fact]
    public async Task Skips_tracks_without_a_preview_without_downloading()
    {
        var outcome = await CreateHandler().HandleAsync(
            new IngestTrackCommand(Track(preview: null), SongOrigin.International), CancellationToken.None);

        outcome.ShouldBe(IngestOutcome.NoPreview);
        _source.DownloadedUrls.ShouldBeEmpty();
    }

    [Fact]
    public async Task Skips_tracks_already_in_the_catalog_without_downloading()
    {
        var handler = CreateHandler();
        await handler.HandleAsync(new IngestTrackCommand(Track(), SongOrigin.International), CancellationToken.None);

        var outcome = await handler.HandleAsync(new IngestTrackCommand(Track(), SongOrigin.International), CancellationToken.None);

        outcome.ShouldBe(IngestOutcome.AlreadyInCatalog);
        _source.DownloadedUrls.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Rejects_previews_that_are_too_short()
    {
        var outcome = await CreateHandler(frames: 50).HandleAsync(
            new IngestTrackCommand(Track(), SongOrigin.International), CancellationToken.None);

        outcome.ShouldBe(IngestOutcome.TooShort);
        _catalog.Songs.ShouldBeEmpty();
    }
}
