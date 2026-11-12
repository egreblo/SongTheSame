using System.Text.Json;

namespace SongTheSame.Spike;

public sealed record DeezerTrack(long Id, string Title, string Artist, string PreviewUrl);

public sealed class DeezerPreviewClient(HttpClient http)
{
    private const string SearchUrl = "https://api.deezer.com/search?limit=5&q=";

    public async Task<DeezerTrack?> FindAsync(string query, CancellationToken ct = default)
    {
        await using var stream = await http.GetStreamAsync(SearchUrl + Uri.EscapeDataString(query), ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        foreach (var item in doc.RootElement.GetProperty("data").EnumerateArray())
        {
            var preview = item.GetProperty("preview").GetString();
            if (string.IsNullOrEmpty(preview)) continue;

            return new DeezerTrack(
                item.GetProperty("id").GetInt64(),
                item.GetProperty("title").GetString() ?? "",
                item.GetProperty("artist").GetProperty("name").GetString() ?? "",
                preview);
        }

        return null;
    }

    public Task<byte[]> DownloadAsync(string url, CancellationToken ct = default) =>
        http.GetByteArrayAsync(url, ct);
}
