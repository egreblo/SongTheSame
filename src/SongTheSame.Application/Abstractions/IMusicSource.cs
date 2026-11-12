namespace SongTheSame.Application.Abstractions;

public sealed record SourceTrack(
    long Id,
    string Title,
    string Artist,
    string? AlbumTitle,
    string? CoverUrl,
    int DurationSeconds,
    int Rank,
    string? PreviewUrl);

/// <summary>External provider of track metadata and 30-second previews (Deezer).</summary>
public interface IMusicSource
{
    Task<IReadOnlyList<SourceTrack>> GetPlaylistTracksAsync(long playlistId, int limit, CancellationToken cancellationToken);

    /// <summary>A currently valid preview URL for a track (Deezer URLs expire after a few hours).</summary>
    Task<string?> GetPreviewUrlAsync(long trackId, CancellationToken cancellationToken);

    Task<byte[]> DownloadPreviewAsync(string previewUrl, CancellationToken cancellationToken);
}
