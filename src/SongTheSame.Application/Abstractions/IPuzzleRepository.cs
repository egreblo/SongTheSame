using SongTheSame.Domain.Puzzles;

namespace SongTheSame.Application.Abstractions;

public interface IPuzzleRepository
{
    Task<Puzzle?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<Puzzle?> GetByCodeAsync(string code, CancellationToken cancellationToken);

    Task<IReadOnlyList<Puzzle>> ListCustomByOwnerAsync(Guid ownerId, CancellationToken cancellationToken);

    /// <summary>The daily puzzle of a game day.</summary>
    Task<Puzzle?> GetByDayAsync(DateOnly day, CancellationToken cancellationToken);

    /// <summary>Song ids used by puzzles in the given number of days before <paramref name="day"/>.</summary>
    Task<HashSet<Guid>> GetRecentSongIdsAsync(DateOnly day, int days, CancellationToken cancellationToken);

    /// <summary>Returns false when a puzzle for that day already exists (for example created concurrently).</summary>
    Task<bool> TryAddAsync(Puzzle puzzle, CancellationToken cancellationToken);
}
