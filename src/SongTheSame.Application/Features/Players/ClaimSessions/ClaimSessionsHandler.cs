using SongTheSame.Application.Abstractions;
using SongTheSame.Domain.Players;

namespace SongTheSame.Application.Features.Players.ClaimSessions;

public sealed record ClaimSessionsCommand(Guid UserId, string AnonymousPlayerKey);

/// <summary>
/// After signing in, moves the games played anonymously in this browser to the account, unless the account
/// already has a game for that puzzle (the existing one wins). Returns how many games were moved.
/// </summary>
public sealed class ClaimSessionsHandler(ISessionRepository sessions) : IHandler<ClaimSessionsCommand, int>
{
    public async Task<int> HandleAsync(ClaimSessionsCommand request, CancellationToken cancellationToken)
    {
        var userKey = PlayerKey.ForUser(request.UserId);
        var anonymous = await sessions.ListByPlayerAsync(request.AnonymousPlayerKey, cancellationToken);
        var existing = await sessions.ListByPlayerAsync(userKey, cancellationToken);
        var takenPuzzles = existing.Select(session => session.PuzzleId).ToHashSet();

        var moved = 0;
        foreach (var session in anonymous.Where(session => takenPuzzles.Add(session.PuzzleId)))
        {
            session.ReassignTo(userKey);
            moved++;
        }

        if (moved > 0) await sessions.SaveAsync(cancellationToken);
        return moved;
    }
}
