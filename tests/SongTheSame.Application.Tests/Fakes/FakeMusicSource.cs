using SongTheSame.Application.Abstractions;

namespace SongTheSame.Application.Tests.Fakes;

internal sealed class FakeMusicSource(params SourceTrack[] tracks) : IMusicSource
{
    public List<string> DownloadedUrls { get; } = [];

    public Task<IReadOnlyList<SourceTrack>> GetPlaylistTracksAsync(
        long playlistId, int limit, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<SourceTrack>>(tracks.Take(limit).ToList());

    public Task<string?> GetPreviewUrlAsync(long trackId, CancellationToken cancellationToken) =>
        Task.FromResult<string?>($"https://cdn/{trackId}.mp3");

    public Task<byte[]> DownloadPreviewAsync(string previewUrl, CancellationToken cancellationToken)
    {
        DownloadedUrls.Add(previewUrl);
        if (previewUrl.Contains("broken", StringComparison.Ordinal)) throw new HttpRequestException("404");
        return Task.FromResult<byte[]>([1, 2, 3]);
    }
}
