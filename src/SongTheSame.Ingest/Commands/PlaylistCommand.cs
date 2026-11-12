using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Catalog.IngestPlaylist;
using SongTheSame.Domain.Songs;

namespace SongTheSame.Ingest.Commands;

public sealed class PlaylistCommand(IHandler<IngestPlaylistCommand, IngestPlaylistResult> ingest)
{
    private const int DefaultLimit = 100;

    public async Task<int> RunAsync(string[] args)
    {
        if (args.Length == 0 || !long.TryParse(args[0], out var playlistId))
        {
            Console.WriteLine("Playlist id is required: playlist <id> [--origin ...] [--limit N]");
            return 1;
        }

        var origin = Enum.TryParse<SongOrigin>(Option(args, "--origin"), ignoreCase: true, out var parsed)
            ? parsed
            : SongOrigin.International;
        var limit = int.TryParse(Option(args, "--limit"), out var parsedLimit) ? parsedLimit : DefaultLimit;

        var result = await ingest.HandleAsync(new IngestPlaylistCommand(playlistId, origin, limit), CancellationToken.None);
        Console.WriteLine($"Playlist {playlistId}: added {result.Added}, skipped {result.Skipped}, failed {result.Failed}");
        return 0;
    }

    private static string? Option(string[] args, string name)
    {
        var index = Array.IndexOf(args, name);
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }
}
