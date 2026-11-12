using Microsoft.EntityFrameworkCore;
using SongTheSame.Application.Abstractions;
using SongTheSame.Domain.Game;
using SongTheSame.Domain.Puzzles;

namespace SongTheSame.Infrastructure.Persistence;

internal sealed class SessionRepository(AppDbContext db) : ISessionRepository
{
    public Task<GameSession?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        db.GameSessions.Include(session => session.Rounds).FirstOrDefaultAsync(session => session.Id == id, cancellationToken);

    public async Task<IReadOnlyList<GameSession>> GetManyAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken) =>
        await db.GameSessions.AsNoTracking().Include(session => session.Rounds)
            .Where(session => ids.Contains(session.Id)).ToListAsync(cancellationToken);

    public Task<GameSession?> FindAsync(string playerKey, Guid puzzleId, CancellationToken cancellationToken) =>
        db.GameSessions.Include(session => session.Rounds)
            .FirstOrDefaultAsync(session => session.PlayerKey == playerKey && session.PuzzleId == puzzleId, cancellationToken);

    public async Task<bool> TryAddAsync(GameSession session, CancellationToken cancellationToken)
    {
        db.GameSessions.Add(session);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (UniqueViolation.IsUniqueViolation(exception))
        {
            db.ChangeTracker.Clear();
            return false;
        }
    }

    public async Task<IReadOnlyList<GameSession>> ListByPlayerAsync(string playerKey, CancellationToken cancellationToken) =>
        await db.GameSessions.Include(session => session.Rounds)
            .Where(session => session.PlayerKey == playerKey).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<DailyResult>> GetDailyResultsAsync(
        IReadOnlyCollection<string> playerKeys, DateOnly firstDay, DateOnly lastDay, CancellationToken cancellationToken)
    {
        var sessions = await db.GameSessions.AsNoTracking().Include(session => session.Rounds)
            .Where(session => playerKeys.Contains(session.PlayerKey) && session.Day >= firstDay && session.Day <= lastDay && session.CompletedAt != null)
            .Where(session => db.Puzzles.Any(puzzle => puzzle.Id == session.PuzzleId && puzzle.Kind == PuzzleKind.Daily))
            .ToListAsync(cancellationToken);
        return sessions.Select(session => new DailyResult(session.PlayerKey, session.Day, session.TotalPoints)).ToList();
    }

    public Task SaveAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);

    public async Task<IReadOnlyList<int>> GetCompletedTotalsAsync(Guid puzzleId, CancellationToken cancellationToken)
    {
        var sessions = await db.GameSessions.AsNoTracking().Include(session => session.Rounds)
            .Where(session => session.PuzzleId == puzzleId && session.CompletedAt != null)
            .ToListAsync(cancellationToken);
        return sessions.Select(session => session.TotalPoints).ToList();
    }
}
