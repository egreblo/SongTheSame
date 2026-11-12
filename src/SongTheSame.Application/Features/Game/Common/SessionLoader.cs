using SongTheSame.Application.Abstractions;
using SongTheSame.Domain.Game;
using SongTheSame.Domain.Puzzles;

namespace SongTheSame.Application.Features.Game.Common;

/// <summary>Loads a session together with its puzzle, hiding sessions that belong to other players.</summary>
public sealed class SessionLoader(ISessionRepository sessions, IPuzzleRepository puzzles)
{
    public async Task<(GameSession Session, Puzzle Puzzle)> LoadAsync(
        Guid sessionId, string playerKey, CancellationToken cancellationToken)
    {
        var session = await sessions.GetAsync(sessionId, cancellationToken);
        if (session is null || session.PlayerKey != playerKey) throw new KeyNotFoundException("Session not found.");

        var puzzle = await puzzles.GetAsync(session.PuzzleId, cancellationToken)
            ?? throw new KeyNotFoundException("Puzzle not found.");
        return (session, puzzle);
    }
}
