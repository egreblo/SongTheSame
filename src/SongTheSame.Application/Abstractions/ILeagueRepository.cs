using SongTheSame.Domain.Leagues;

namespace SongTheSame.Application.Abstractions;

public interface ILeagueRepository
{
    /// <summary>Tracked league with its members.</summary>
    Task<League?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<League?> GetByInviteCodeAsync(string code, CancellationToken cancellationToken);

    Task<IReadOnlyList<League>> ListForUserAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Returns false when the invite code is already taken.</summary>
    Task<bool> TryAddAsync(League league, CancellationToken cancellationToken);

    Task SaveAsync(CancellationToken cancellationToken);
}
