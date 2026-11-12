using SongTheSame.Application.Abstractions;
using SongTheSame.Audio;
using SongTheSame.Domain.Songs;

namespace SongTheSame.Application.Features.Catalog.IngestTrack;

/// <summary>Downloads a track's preview, extracts chroma features and stores both song and features.</summary>
public sealed class IngestTrackHandler(
    ISongCatalog catalog,
    IMusicSource source,
    IChromaExtractor extractor,
    TimeProvider clock) : IHandler<IngestTrackCommand, IngestOutcome>
{
    /// <summary>A usable preview must hold at least this many chroma frames (about 20 s at 10 fps).</summary>
    private const int MinimumFrames = 200;

    public async Task<IngestOutcome> HandleAsync(IngestTrackCommand request, CancellationToken cancellationToken)
    {
        var track = request.Track;
        if (string.IsNullOrEmpty(track.PreviewUrl)) return IngestOutcome.NoPreview;
        if (await catalog.ExistsAsync(track.Id, cancellationToken)) return IngestOutcome.AlreadyInCatalog;

        var audio = await source.DownloadPreviewAsync(track.PreviewUrl, cancellationToken);
        var chroma = extractor.Extract(audio);
        if (chroma.FrameCount < MinimumFrames) return IngestOutcome.TooShort;

        var song = Song.Create(
            track.Id, track.Title, track.Artist, track.AlbumTitle, track.CoverUrl,
            track.DurationSeconds, track.Rank, request.Origin, clock.GetUtcNow());
        var features = SongFeatures.Create(
            song.Id, ChromaCodec.CurrentVersion, chroma.FramesPerSecond, chroma.FrameCount, ChromaCodec.Encode(chroma));

        await catalog.AddAsync(song, features, cancellationToken);
        return IngestOutcome.Added;
    }
}
