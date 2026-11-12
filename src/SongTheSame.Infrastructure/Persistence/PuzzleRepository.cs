using Microsoft.EntityFrameworkCore;
using SongTheSame.Application.Abstractions;
using SongTheSame.Domain.Puzzles;

namespace SongTheSame.Infrastructure.Persistence;

internal sealed class PuzzleRepository(AppDbContext db) : IPuzzleRepository
{
    public Task<Puzzle?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        db.Puzzles.Include(puzzle => puzzle.Rounds).AsNoTracking()
            .FirstOrDefaultAsync(puzzle => puzzle.Id == id, cancellationToken);

    public Task<Puzzle?> GetByDayAsync(DateOnly day, CancellationToken cancellationToken) =>
        db.Puzzles.Include(puzzle => puzzle.Rounds).AsNoTracking()
            .FirstOrDefaultAsync(puzzle => puzzle.Kind == PuzzleKind.Daily && puzzle.Day == day, cancellationToken);

    public Task<Puzzle?> GetByCodeAsync(string code, CancellationToken cancellationToken) =>
        db.Puzzles.Include(puzzle => puzzle.Rounds).AsNoTracking()
            .FirstOrDefaultAsync(puzzle => puzzle.Code == code, cancellationToken);

    public async Task<IReadOnlyList<Puzzle>> ListCustomByOwnerAsync(Guid ownerId, CancellationToken cancellationToken) =>
        await db.Puzzles.AsNoTracking()
            .Where(puzzle => puzzle.Kind == PuzzleKind.Custom && puzzle.OwnerId == ownerId)
            .OrderByDescending(puzzle => puzzle.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<HashSet<Guid>> GetRecentSongIdsAsync(DateOnly day, int days, CancellationToken cancellationToken)
    {
        var from = day.AddDays(-days);
        var ids = await db.Puzzles.AsNoTracking()
            .Where(puzzle => puzzle.Kind == PuzzleKind.Daily && puzzle.Day >= from && puzzle.Day < day)
            .SelectMany(puzzle => puzzle.Rounds.Select(round => round.SongId))
            .ToListAsync(cancellationToken);
        return [.. ids];
    }

    public async Task<bool> TryAddAsync(Puzzle puzzle, CancellationToken cancellationToken)
    {
        db.Puzzles.Add(puzzle);
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
}
