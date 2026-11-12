using Microsoft.EntityFrameworkCore;
using SongTheSame.Application.Abstractions;
using SongTheSame.Domain.Players;

namespace SongTheSame.Infrastructure.Persistence;

internal sealed class ProfileRepository(AppDbContext db) : IProfileRepository
{
    public Task<Profile?> GetAsync(Guid userId, CancellationToken cancellationToken) =>
        db.Profiles.FirstOrDefaultAsync(profile => profile.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Profile>> GetManyAsync(IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken) =>
        await db.Profiles.AsNoTracking().Where(profile => userIds.Contains(profile.UserId)).ToListAsync(cancellationToken);

    public async Task<bool> TryAddAsync(Profile profile, CancellationToken cancellationToken)
    {
        db.Profiles.Add(profile);
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
