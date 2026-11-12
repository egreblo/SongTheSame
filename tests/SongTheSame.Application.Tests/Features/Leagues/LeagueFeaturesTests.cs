using Microsoft.Extensions.Time.Testing;
using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Leagues.CreateLeague;
using SongTheSame.Application.Features.Leagues.GetLeagueStandings;
using SongTheSame.Application.Features.Leagues.JoinLeague;
using SongTheSame.Application.Features.Leagues.LeaveLeague;
using SongTheSame.Application.Features.Leagues.ListMyLeagues;
using SongTheSame.Application.Tests.Fakes;
using SongTheSame.Domain.Game;
using SongTheSame.Domain.Leagues;
using SongTheSame.Domain.Players;
using SongTheSame.Domain.Puzzles;

namespace SongTheSame.Application.Tests.Features.Leagues;

public class LeagueFeaturesTests
{
    private static readonly Guid Ana = Guid.Parse("00000000-0000-4000-8000-00000000000a");
    private static readonly Guid Ivo = Guid.Parse("00000000-0000-4000-8000-00000000000b");
    private static readonly Guid Outsider = Guid.Parse("00000000-0000-4000-8000-00000000000c");

    private readonly InMemoryLeagueRepository _leagues = new();
    private readonly InMemorySessionRepository _sessions = new();
    private readonly InMemoryProfileRepository _profiles = new();
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 11, 13, 10, 0, 0, TimeSpan.Zero));

    private static CancellationToken Ct => CancellationToken.None;

    private async Task<Application.Features.Leagues.LeagueSummary> CreateAsync() =>
        await new CreateLeagueHandler(_leagues, _clock).HandleAsync(new CreateLeagueCommand(Ana, "Office"), Ct);

    private void AddFinishedGame(Guid user, DateOnly day, int perfectRounds)
    {
        var rounds = Enumerable.Range(0, 5).Select(i => PuzzleRound.Create(i, Guid.NewGuid(), 50, 5, 6, []));
        var puzzle = Puzzle.CreateDaily(day, rounds, DateTimeOffset.UnixEpoch);
        var session = GameSession.Start(PlayerKey.ForUser(user), puzzle, DateTimeOffset.UnixEpoch);
        for (var i = 0; i < 5; i++)
        {
            session.RegisterPhase1Attempt(i, Guid.NewGuid(), correct: true);
            session.RegisterPhase2(i, Guid.NewGuid(), i < perfectRounds ? 100 : 1, DateTimeOffset.UnixEpoch);
        }

        _sessions.Sessions.Add(session);
    }

    [Fact]
    public async Task Creating_a_league_makes_the_creator_the_owner_and_only_member()
    {
        var league = await CreateAsync();

        league.IsOwner.ShouldBeTrue();
        league.MemberCount.ShouldBe(1);
        league.InviteCode.Length.ShouldBe(8);
    }

    [Fact]
    public async Task Joining_works_with_a_sloppily_typed_code_and_twice_is_harmless()
    {
        var league = await CreateAsync();
        var join = new JoinLeagueHandler(_leagues, _clock);
        var typed = league.InviteCode[..4].ToLowerInvariant() + " - " + league.InviteCode[4..];

        await join.HandleAsync(new JoinLeagueCommand(Ivo, typed), Ct);
        var again = await join.HandleAsync(new JoinLeagueCommand(Ivo, league.InviteCode), Ct);

        again.MemberCount.ShouldBe(2);
        again.IsOwner.ShouldBeFalse();
    }

    [Fact]
    public async Task An_unknown_invite_code_is_not_found()
    {
        await Should.ThrowAsync<KeyNotFoundException>(
            () => new JoinLeagueHandler(_leagues, _clock).HandleAsync(new JoinLeagueCommand(Ivo, "NOSUCHCD"), Ct));
    }

    [Fact]
    public async Task Members_see_the_league_in_their_list_and_non_members_do_not()
    {
        var league = await CreateAsync();
        await new JoinLeagueHandler(_leagues, _clock).HandleAsync(new JoinLeagueCommand(Ivo, league.InviteCode), Ct);
        var list = new ListMyLeaguesHandler(_leagues);

        (await list.HandleAsync(new ListMyLeaguesQuery(Ivo), Ct)).ShouldHaveSingleItem().Name.ShouldBe("Office");
        (await list.HandleAsync(new ListMyLeaguesQuery(Outsider), Ct)).ShouldBeEmpty();
    }

    [Fact]
    public async Task Standings_rank_members_by_their_week_and_show_todays_result_when_finished()
    {
        var league = await CreateAsync();
        await new JoinLeagueHandler(_leagues, _clock).HandleAsync(new JoinLeagueCommand(Ivo, league.InviteCode), Ct);
        await _profiles.TryAddAsync(Profile.Create(Ana, "Ana", _clock.GetUtcNow()), Ct);
        await _profiles.TryAddAsync(Profile.Create(Ivo, "Ivo", _clock.GetUtcNow()), Ct);

        AddFinishedGame(Ana, new DateOnly(2026, 11, 13), perfectRounds: 5);
        AddFinishedGame(Ana, new DateOnly(2026, 11, 12), perfectRounds: 5);
        AddFinishedGame(Ivo, new DateOnly(2026, 11, 12), perfectRounds: 0);
        AddFinishedGame(Ana, new DateOnly(2026, 10, 1), perfectRounds: 5); // outside both windows

        var standings = await new GetLeagueStandingsHandler(_leagues, _sessions, _profiles, _clock)
            .HandleAsync(new GetLeagueStandingsQuery(Ana, league.Id), Ct);

        standings.Today.ShouldBe("2026-11-13");
        standings.Rows.Select(row => row.DisplayName).ShouldBe(["Ana", "Ivo"]);
        standings.Rows[0].TodayPoints.ShouldBe(500);
        standings.Rows[0].WeekPoints.ShouldBe(1000);
        standings.Rows[0].GamesThisWeek.ShouldBe(2);
        standings.Rows[1].TodayPoints.ShouldBeNull();
        standings.Rows[1].WeekPoints.ShouldBe(255); // 5 x 50 for phase 1 + 5 x 1 for phase 2
    }

    [Fact]
    public async Task Non_members_cannot_see_standings()
    {
        var league = await CreateAsync();

        await Should.ThrowAsync<KeyNotFoundException>(
            () => new GetLeagueStandingsHandler(_leagues, _sessions, _profiles, _clock)
                .HandleAsync(new GetLeagueStandingsQuery(Outsider, league.Id), Ct));
    }

    [Fact]
    public async Task Members_can_leave_but_the_owner_cannot()
    {
        var league = await CreateAsync();
        await new JoinLeagueHandler(_leagues, _clock).HandleAsync(new JoinLeagueCommand(Ivo, league.InviteCode), Ct);
        var leave = new LeaveLeagueHandler(_leagues);

        await leave.HandleAsync(new LeaveLeagueCommand(Ivo, league.Id), Ct);
        _leagues.Leagues.Single().IsMember(Ivo).ShouldBeFalse();

        await Should.ThrowAsync<GameRuleException>(() => leave.HandleAsync(new LeaveLeagueCommand(Ana, league.Id), Ct));
    }

    [Fact]
    public void A_league_has_a_member_limit()
    {
        var league = League.Create("Big", Ana, "ABCDEFGH", DateTimeOffset.UnixEpoch);
        for (var i = 1; i < League.MaxMembers; i++) league.Join(Guid.NewGuid(), DateTimeOffset.UnixEpoch);

        Should.Throw<GameRuleException>(() => league.Join(Guid.NewGuid(), DateTimeOffset.UnixEpoch));
    }
}

internal sealed class InMemoryLeagueRepository : ILeagueRepository
{
    public List<League> Leagues { get; } = [];

    public Task<League?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Leagues.FirstOrDefault(league => league.Id == id));

    public Task<League?> GetByInviteCodeAsync(string code, CancellationToken cancellationToken) =>
        Task.FromResult(Leagues.FirstOrDefault(league => league.InviteCode == code));

    public Task<IReadOnlyList<League>> ListForUserAsync(Guid userId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<League>>(Leagues.Where(league => league.IsMember(userId)).ToList());

    public Task<bool> TryAddAsync(League league, CancellationToken cancellationToken)
    {
        if (Leagues.Any(existing => existing.InviteCode == league.InviteCode)) return Task.FromResult(false);
        Leagues.Add(league);
        return Task.FromResult(true);
    }

    public Task SaveAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
