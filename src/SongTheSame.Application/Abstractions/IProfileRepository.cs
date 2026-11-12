using SongTheSame.Domain.Players;

namespace SongTheSame.Application.Abstractions;

public interface IProfileRepository
{
    Task<Profile?> GetAsync(Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Profile>> GetManyAsync(IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken);

    /// <summary>Returns false when the profile already exists (created concurrently).</summary>
    Task<bool> TryAddAsync(Profile profile, CancellationToken cancellationToken);

    Task SaveAsync(CancellationToken cancellationToken);
}
