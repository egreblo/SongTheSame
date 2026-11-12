using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Game.Common;
using SongTheSame.Domain.Common;

namespace SongTheSame.Application.Features.CustomGames.StartCustomSession;

public sealed record StartCustomSessionCommand(string PlayerKey, string Code);

/// <summary>Starts (or resumes) a player's game of a shared custom puzzle.</summary>
public sealed class StartCustomSessionHandler(IPuzzleRepository puzzles, SessionStarter starter)
    : IHandler<StartCustomSessionCommand, SessionView>
{
    public async Task<SessionView> HandleAsync(StartCustomSessionCommand request, CancellationToken cancellationToken)
    {
        var puzzle = await puzzles.GetByCodeAsync(ShareCode.Normalize(request.Code), cancellationToken)
            ?? throw new KeyNotFoundException("Game not found.");
        return await starter.StartAsync(request.PlayerKey, puzzle, cancellationToken);
    }
}
