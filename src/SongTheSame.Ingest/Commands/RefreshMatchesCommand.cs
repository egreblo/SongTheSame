using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Game.RefreshMatches;
using SongTheSame.Domain.Common;

namespace SongTheSame.Ingest.Commands;

public sealed class RefreshMatchesCommand(IHandler<Application.Features.Game.RefreshMatches.RefreshMatchesCommand, int> refresh, TimeProvider clock)
{
    public async Task<int> RunAsync(string[] args)
    {
        var offset = args.Length > 0 && int.TryParse(args[0], out var parsed) ? parsed : 0;
        var from = GameDay.From(clock.GetUtcNow()).Date.AddDays(offset);

        var rounds = await refresh.HandleAsync(new Application.Features.Game.RefreshMatches.RefreshMatchesCommand(from), CancellationToken.None);
        Console.WriteLine($"Refreshed the best twins of {rounds} puzzle rounds from {from:yyyy-MM-dd} onward.");
        return 0;
    }
}
