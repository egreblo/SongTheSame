using SongTheSame.Application.Abstractions;
using SongTheSame.Domain.Common;
using SongTheSame.Domain.Leagues;

namespace SongTheSame.Application.Features.Leagues.CreateLeague;

public sealed record CreateLeagueCommand(Guid UserId, string Name);

public sealed class CreateLeagueHandler(ILeagueRepository leagues, TimeProvider clock) : IHandler<CreateLeagueCommand, LeagueSummary>
{
    private const int MaxLeaguesPerUser = 20;
    private const int CodeAttempts = 5;

    public async Task<LeagueSummary> HandleAsync(CreateLeagueCommand request, CancellationToken cancellationToken)
    {
        var mine = await leagues.ListForUserAsync(request.UserId, cancellationToken);
        if (mine.Count >= MaxLeaguesPerUser) throw new Domain.Game.GameRuleException("You are in too many leagues already.");

        for (var attempt = 0; attempt < CodeAttempts; attempt++)
        {
            var league = League.Create(request.Name, request.UserId, ShareCode.Generate(), clock.GetUtcNow());
            if (await leagues.TryAddAsync(league, cancellationToken))
                return new LeagueSummary(league.Id, league.Name, league.InviteCode, league.Members.Count, IsOwner: true);
        }

        throw new InvalidOperationException("Could not allocate an invite code.");
    }
}
