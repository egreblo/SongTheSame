namespace SongTheSame.Application.Features.Leagues;

public sealed record LeagueSummary(Guid Id, string Name, string InviteCode, int MemberCount, bool IsOwner);

/// <param name="TodayPoints">Null until the member has finished today's puzzle.</param>
public sealed record StandingsRow(Guid UserId, string DisplayName, int? TodayPoints, int WeekPoints, int MonthPoints, int GamesThisWeek);

public sealed record LeagueStandings(Guid Id, string Name, string InviteCode, string Today, IReadOnlyList<StandingsRow> Rows);
