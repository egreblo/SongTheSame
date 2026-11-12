namespace SongTheSame.Domain.Songs;

/// <summary>
/// Precomputed, quantised chroma sequence of a song's preview. Opaque to the domain:
/// only the audio library knows how to encode and decode <see cref="Data"/>.
/// </summary>
public sealed class SongFeatures
{
    private SongFeatures()
    {
    }

    public Guid SongId { get; private set; }

    /// <summary>Bumped whenever the extraction algorithm changes, so stale rows can be recomputed.</summary>
    public int ExtractorVersion { get; private set; }

    public double FramesPerSecond { get; private set; }

    public int FrameCount { get; private set; }

    public byte[] Data { get; private set; } = [];

    public static SongFeatures Create(Guid songId, int extractorVersion, double framesPerSecond, int frameCount, byte[] data) =>
        new()
        {
            SongId = songId,
            ExtractorVersion = extractorVersion,
            FramesPerSecond = framesPerSecond,
            FrameCount = frameCount,
            Data = data,
        };
}
