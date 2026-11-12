using SongTheSame.Domain.Game;

namespace SongTheSame.Application.Abstractions;

public sealed record DailyResult(string PlayerKey, DateOnly Day, int Points);

public interface ISessionRepository
{
    Task<GameSession?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<GameSession?> FindAsync(string playerKey, Guid puzzleId, CancellationToken cancellationToken);

    Task<bool> TryAddAsync(GameSession session, CancellationToken cancellationToken);

    /// <summary>Persists changes made to sessions loaded from this repository.</summary>
    Task SaveAsync(CancellationToken cancellationToken);

    /// <summary>All sessions of a player key, tracked for modification.</summary>
    Task<IReadOnlyList<GameSession>> ListByPlayerAsync(string playerKey, CancellationToken cancellationToken);

    /// <summary>Finished games of the daily puzzle for the given players between two game days (inclusive).</summary>
    Task<IReadOnlyList<DailyResult>> GetDailyResultsAsync(
        IReadOnlyCollection<string> playerKeys, DateOnly firstDay, DateOnly lastDay, CancellationToken cancellationToken);

    /// <summary>Total points of every completed session of a puzzle, for comparing players.</summary>
    Task<IReadOnlyList<int>> GetCompletedTotalsAsync(Guid puzzleId, CancellationToken cancellationToken);
}
