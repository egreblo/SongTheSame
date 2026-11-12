using Microsoft.EntityFrameworkCore;
using SongTheSame.Application.Abstractions;
using SongTheSame.Domain.Leagues;

namespace SongTheSame.Infrastructure.Persistence;

internal sealed class LeagueRepository(AppDbContext db) : ILeagueRepository
{
    public Task<League?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        db.Leagues.Include(league => league.Members).FirstOrDefaultAsync(league => league.Id == id, cancellationToken);

    public Task<League?> GetByInviteCodeAsync(string code, CancellationToken cancellationToken) =>
        db.Leagues.Include(league => league.Members).FirstOrDefaultAsync(league => league.InviteCode == code, cancellationToken);

    public async Task<IReadOnlyList<League>> ListForUserAsync(Guid userId, CancellationToken cancellationToken) =>
        await db.Leagues.AsNoTracking().Include(league => league.Members)
            .Where(league => league.Members.Any(member => member.UserId == userId))
            .OrderBy(league => league.Name)
            .ToListAsync(cancellationToken);

    public async Task<bool> TryAddAsync(League league, CancellationToken cancellationToken)
    {
        db.Leagues.Add(league);
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

    public Task SaveAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
