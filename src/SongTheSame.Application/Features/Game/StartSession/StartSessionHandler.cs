using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Game.Common;
using SongTheSame.Domain.Common;

namespace SongTheSame.Application.Features.Game.StartSession;

public sealed record StartSessionCommand(string PlayerKey);

/// <summary>Starts today's game for a player, or returns the one already in progress.</summary>
public sealed class StartSessionHandler(
    DailyPuzzleProvider puzzles,
    SessionStarter starter,
    TimeProvider clock) : IHandler<StartSessionCommand, SessionView>
{
    public async Task<SessionView> HandleAsync(StartSessionCommand request, CancellationToken cancellationToken)
    {
        var puzzle = await puzzles.GetOrCreateAsync(GameDay.From(clock.GetUtcNow()).Date, cancellationToken);
        return await starter.StartAsync(request.PlayerKey, puzzle, cancellationToken);
    }
}
