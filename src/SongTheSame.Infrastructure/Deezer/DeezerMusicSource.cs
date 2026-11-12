using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using SongTheSame.Application.Abstractions;

namespace SongTheSame.Infrastructure.Deezer;

internal sealed class DeezerMusicSource(HttpClient http, IMemoryCache cache) : IMusicSource
{
    private const int PageSize = 100;
    private const int QuotaExceededCode = 4;
    private const int MaxQuotaRetries = 4;
    private static readonly TimeSpan PreviewUrlLifetime = TimeSpan.FromMinutes(20);

    public async Task<IReadOnlyList<SourceTrack>> GetPlaylistTracksAsync(
        long playlistId, int limit, CancellationToken cancellationToken)
    {
        var tracks = new List<SourceTrack>();

        for (var index = 0; tracks.Count < limit; index += PageSize)
        {
            var url = $"https://api.deezer.com/playlist/{playlistId}/tracks?limit={PageSize}&index={index}";
            var page = await GetPageAsync(url, cancellationToken);
            if (page.Data is null or { Count: 0 }) break;

            tracks.AddRange(page.Data.Select(ToSourceTrack));
            if (page.Next is null) break;
        }

        return tracks.Take(limit).ToList();
    }

    public async Task<string?> GetPreviewUrlAsync(long trackId, CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(trackId, out string? cached)) return cached;

        var track = await http.GetFromJsonAsync<DeezerTrackDto>($"https://api.deezer.com/track/{trackId}", cancellationToken);
        var url = string.IsNullOrEmpty(track?.Preview) ? null : track.Preview;
        if (url is not null) cache.Set(trackId, url, PreviewUrlLifetime);
        return url;
    }

    public async Task<byte[]> DownloadPreviewAsync(string previewUrl, CancellationToken cancellationToken) =>
        await http.GetByteArrayAsync(previewUrl, cancellationToken);

    /// <summary>Deezer reports rate limiting as HTTP 200 with an error body, so it is retried here.</summary>
    private async Task<DeezerPage> GetPageAsync(string url, CancellationToken cancellationToken)
    {
        for (var attempt = 0; ; attempt++)
        {
            var page = await http.GetFromJsonAsync<DeezerPage>(url, cancellationToken)
                ?? throw new InvalidDataException("Empty response from Deezer.");

            if (page.Error is null) return page;
            if (page.Error.Code != QuotaExceededCode || attempt >= MaxQuotaRetries)
                throw new InvalidOperationException($"Deezer error {page.Error.Code}: {page.Error.Message}");

            await Task.Delay(TimeSpan.FromSeconds(5 * (attempt + 1)), cancellationToken);
        }
    }

    private static SourceTrack ToSourceTrack(DeezerTrackDto dto) => new(
        dto.Id,
        dto.Title ?? "",
        dto.Artist?.Name ?? "",
        dto.Album?.Title,
        dto.Album?.CoverMedium,
        dto.Duration,
        dto.Rank,
        string.IsNullOrEmpty(dto.Preview) ? null : dto.Preview);
}
