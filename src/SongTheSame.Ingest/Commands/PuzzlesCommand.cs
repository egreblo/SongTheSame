using SongTheSame.Application.Features.Game.Common;
using SongTheSame.Domain.Common;

namespace SongTheSame.Ingest.Commands;

/// <summary>Generates upcoming daily puzzles ahead of time so no player waits for generation.</summary>
public sealed class PuzzlesCommand(DailyPuzzleProvider provider, TimeProvider clock)
{
    private const int DefaultDays = 7;

    public async Task<int> RunAsync(string[] args)
    {
        var days = args.Length > 0 && int.TryParse(args[0], out var parsed) ? parsed : DefaultDays;
        var today = GameDay.From(clock.GetUtcNow()).Date;

        for (var offset = 0; offset < days; offset++)
        {
            var day = today.AddDays(offset);
            var puzzle = await provider.GetOrCreateAsync(day, CancellationToken.None);
            Console.WriteLine($"{day:yyyy-MM-dd}: {puzzle.Rounds.Count} rounds ready");
        }

        return 0;
    }
}
