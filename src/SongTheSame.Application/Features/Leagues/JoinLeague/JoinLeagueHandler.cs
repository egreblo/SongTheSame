using SongTheSame.Application.Abstractions;
using SongTheSame.Domain.Common;

namespace SongTheSame.Application.Features.Leagues.JoinLeague;

public sealed record JoinLeagueCommand(Guid UserId, string InviteCode);

public sealed class JoinLeagueHandler(ILeagueRepository leagues, TimeProvider clock) : IHandler<JoinLeagueCommand, LeagueSummary>
{
    public async Task<LeagueSummary> HandleAsync(JoinLeagueCommand request, CancellationToken cancellationToken)
    {
        var league = await leagues.GetByInviteCodeAsync(ShareCode.Normalize(request.InviteCode), cancellationToken)
            ?? throw new KeyNotFoundException("No league has that invite code.");

        league.Join(request.UserId, clock.GetUtcNow());
        await leagues.SaveAsync(cancellationToken);
        return new LeagueSummary(league.Id, league.Name, league.InviteCode, league.Members.Count, league.OwnerId == request.UserId);
    }
}
