using SongTheSame.Application.Abstractions;

namespace SongTheSame.Application.Features.Leagues.ListMyLeagues;

public sealed record ListMyLeaguesQuery(Guid UserId);

public sealed class ListMyLeaguesHandler(ILeagueRepository leagues) : IHandler<ListMyLeaguesQuery, IReadOnlyList<LeagueSummary>>
{
    public async Task<IReadOnlyList<LeagueSummary>> HandleAsync(ListMyLeaguesQuery request, CancellationToken cancellationToken)
    {
        var mine = await leagues.ListForUserAsync(request.UserId, cancellationToken);
        return mine
            .Select(league => new LeagueSummary(league.Id, league.Name, league.InviteCode, league.Members.Count, league.OwnerId == request.UserId))
            .ToList();
    }
}
