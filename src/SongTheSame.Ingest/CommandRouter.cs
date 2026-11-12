using SongTheSame.Ingest.Commands;

namespace SongTheSame.Ingest;

public sealed class CommandRouter(
    MigrateCommand migrate,
    PlaylistCommand playlist,
    SeedCommand seed,
    CalibrateCommand calibrate,
    PuzzlesCommand puzzles,
    RefreshMatchesCommand refreshMatches)
{
    private const string Usage = """
        Usage:
          migrate                                         Apply database migrations
          playlist <id> [--origin international|regional] [--limit N]
                                                          Ingest the tracks of one Deezer playlist
          calibrate [samples]                             Measure random-pair costs used to turn similarity into 1-100 scores
          puzzles [days]                                  Pre-generate daily puzzles starting today (default 7 days)
          refresh-matches [dayOffset]                     Recompute stored best twins of puzzles from today (+offset) onward
          seed [path]                                     Ingest every playlist in the seed file
                                                          (default: seed/playlists.json)
        Connection string: set ConnectionStrings__Database or use `dotnet user-secrets`.
        """;

    public async Task<int> RunAsync(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine(Usage);
            return 1;
        }

        var rest = args[1..];
        switch (args[0])
        {
            case "migrate": return await migrate.RunAsync();
            case "playlist": return await playlist.RunAsync(rest);
            case "seed": return await seed.RunAsync(rest);
            case "calibrate": return await calibrate.RunAsync(rest);
            case "puzzles": return await puzzles.RunAsync(rest);
            case "refresh-matches": return await refreshMatches.RunAsync(rest);
            default:
                Console.WriteLine(Usage);
                return 1;
        }
    }
}
