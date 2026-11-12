using SongTheSame.Application.Abstractions;

namespace SongTheSame.Application.Features.Leagues.LeaveLeague;

public sealed record LeaveLeagueCommand(Guid UserId, Guid LeagueId);

public sealed class LeaveLeagueHandler(ILeagueRepository leagues) : IHandler<LeaveLeagueCommand, bool>
{
    public async Task<bool> HandleAsync(LeaveLeagueCommand request, CancellationToken cancellationToken)
    {
        var league = await leagues.GetAsync(request.LeagueId, cancellationToken);
        if (league is null || !league.IsMember(request.UserId)) throw new KeyNotFoundException("League not found.");

        league.Leave(request.UserId);
        await leagues.SaveAsync(cancellationToken);
        return true;
    }
}
