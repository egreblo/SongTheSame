using SongTheSame.Application.Abstractions;
using SongTheSame.Domain.Game;
using SongTheSame.Domain.Puzzles;

namespace SongTheSame.Application.Features.Game.Common;

/// <summary>Finds the player's game for a puzzle, or starts one. Safe against two requests racing.</summary>
public sealed class SessionStarter(ISessionRepository sessions, SessionViewBuilder views, TimeProvider clock)
{
    public async Task<SessionView> StartAsync(string playerKey, Puzzle puzzle, CancellationToken cancellationToken)
    {
        var session = await sessions.FindAsync(playerKey, puzzle.Id, cancellationToken);
        if (session is null)
        {
            session = GameSession.Start(playerKey, puzzle, clock.GetUtcNow());
            if (!await sessions.TryAddAsync(session, cancellationToken))
                session = await sessions.FindAsync(playerKey, puzzle.Id, cancellationToken)
                    ?? throw new InvalidOperationException("Session could not be created.");
        }

        return await views.BuildAsync(session, puzzle, cancellationToken);
    }
}
