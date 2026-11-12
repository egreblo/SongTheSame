using System.Text.Json;

namespace SongTheSame.Spike;

/// <summary>Downloads previews once and keeps mp3 + metadata on disk.</summary>
public sealed class PreviewCache(string directory, DeezerPreviewClient deezer)
{
    public string Mp3Path(string key) => Path.Combine(directory, key + ".mp3");

    private string MetaPath(string key) => Path.Combine(directory, key + ".json");

    public async Task FetchAllAsync(IEnumerable<SpikeSong> songs)
    {
        Directory.CreateDirectory(directory);
        foreach (var song in songs)
        {
            if (File.Exists(Mp3Path(song.Key)) && File.Exists(MetaPath(song.Key)))
            {
                Console.WriteLine($"cached  {song.Key,-24} {Describe(song.Key)}");
                continue;
            }

            var track = await deezer.FindAsync(song.Query);
            if (track is null)
            {
                Console.WriteLine($"MISSING {song.Key,-24} (no preview for '{song.Query}')");
                continue;
            }

            await File.WriteAllBytesAsync(Mp3Path(song.Key), await deezer.DownloadAsync(track.PreviewUrl));
            await File.WriteAllTextAsync(MetaPath(song.Key), JsonSerializer.Serialize(track with { PreviewUrl = "" }));
            Console.WriteLine($"fetched {song.Key,-24} {track.Artist} - {track.Title}");
            await Task.Delay(200);
        }
    }

    public IReadOnlyList<string> AvailableKeys(IEnumerable<SpikeSong> songs) =>
        songs.Where(s => File.Exists(Mp3Path(s.Key))).Select(s => s.Key).ToList();

    private string Describe(string key)
    {
        var track = JsonSerializer.Deserialize<DeezerTrack>(File.ReadAllText(MetaPath(key)));
        return track is null ? "" : $"{track.Artist} - {track.Title}";
    }
}
