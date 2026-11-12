using SongTheSame.Application.Abstractions;
using SongTheSame.Domain.Common;
using SongTheSame.Domain.Players;

namespace SongTheSame.Application.Features.Leagues.GetLeagueStandings;

public sealed record GetLeagueStandingsQuery(Guid UserId, Guid LeagueId);

/// <summary>Compares the members' results of the daily puzzle: today, the last 7 days and the last 30 days.</summary>
public sealed class GetLeagueStandingsHandler(
    ILeagueRepository leagues,
    ISessionRepository sessions,
    IProfileRepository profiles,
    TimeProvider clock) : IHandler<GetLeagueStandingsQuery, LeagueStandings>
{
    private const int WeekDays = 7;
    private const int MonthDays = 30;

    public async Task<LeagueStandings> HandleAsync(GetLeagueStandingsQuery request, CancellationToken cancellationToken)
    {
        var league = await leagues.GetAsync(request.LeagueId, cancellationToken);
        if (league is null || !league.IsMember(request.UserId)) throw new KeyNotFoundException("League not found.");

        var today = GameDay.From(clock.GetUtcNow()).Date;
        var memberIds = league.Members.Select(member => member.UserId).ToList();
        var results = await sessions.GetDailyResultsAsync(
            memberIds.Select(PlayerKey.ForUser).ToList(), today.AddDays(1 - MonthDays), today, cancellationToken);
        var names = (await profiles.GetManyAsync(memberIds, cancellationToken)).ToDictionary(profile => profile.UserId, profile => profile.DisplayName);

        var weekStart = today.AddDays(1 - WeekDays);
        var rows = memberIds.Select(userId =>
        {
            var mine = results.Where(result => result.PlayerKey == PlayerKey.ForUser(userId)).ToList();
            var week = mine.Where(result => result.Day >= weekStart).ToList();
            int? todayPoints = mine.FirstOrDefault(result => result.Day == today)?.Points;
            return new StandingsRow(
                userId, names.GetValueOrDefault(userId, "Player"), todayPoints, week.Sum(r => r.Points), mine.Sum(r => r.Points), week.Count);
        });

        var ordered = rows.OrderByDescending(row => row.WeekPoints).ThenByDescending(row => row.TodayPoints ?? -1).ThenBy(row => row.DisplayName).ToList();
        return new LeagueStandings(league.Id, league.Name, league.InviteCode, today.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture), ordered);
    }
}
