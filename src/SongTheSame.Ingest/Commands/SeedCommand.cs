using System.Text.Json;
using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Catalog.IngestPlaylist;
using SongTheSame.Domain.Songs;

namespace SongTheSame.Ingest.Commands;

public sealed class SeedCommand(IHandler<IngestPlaylistCommand, IngestPlaylistResult> ingest)
{
    private const string DefaultPath = "seed/playlists.json";

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<int> RunAsync(string[] args)
    {
        var path = args.Length > 0 ? args[0] : DefaultPath;
        if (!File.Exists(path))
        {
            Console.WriteLine($"Seed file not found: {path}");
            return 1;
        }

        var seed = JsonSerializer.Deserialize<SeedFile>(await File.ReadAllTextAsync(path), JsonOptions)
            ?? throw new InvalidDataException("Seed file is empty.");

        var totalAdded = 0;
        foreach (var playlist in seed.Playlists)
        {
            Console.WriteLine($"== {playlist.Label} ({playlist.Id}, {playlist.Origin})");
            var origin = Enum.Parse<SongOrigin>(playlist.Origin, ignoreCase: true);
            var result = await ingest.HandleAsync(
                new IngestPlaylistCommand(playlist.Id, origin, playlist.Limit ?? seed.DefaultLimit), CancellationToken.None);
            Console.WriteLine($"   added {result.Added}, skipped {result.Skipped}, failed {result.Failed}");
            totalAdded += result.Added;
        }

        Console.WriteLine($"Done. {totalAdded} songs added.");
        return 0;
    }
}
