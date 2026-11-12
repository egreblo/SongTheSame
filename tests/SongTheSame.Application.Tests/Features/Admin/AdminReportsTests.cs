using Microsoft.Extensions.Time.Testing;
using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Admin.ListReports;
using SongTheSame.Application.Features.Admin.ReviewReport;
using SongTheSame.Application.Features.Catalog.GetSongPreview;
using SongTheSame.Application.Features.Game.Common;
using SongTheSame.Application.Features.Game.ReportRound;
using SongTheSame.Application.Tests.Fakes;
using SongTheSame.Domain.Game;
using SongTheSame.Domain.Puzzles;
using SongTheSame.Domain.Reports;
using SongTheSame.Domain.Songs;

namespace SongTheSame.Application.Tests.Features.Admin;

public class AdminReportsTests
{
    private const string Player = "anon:11111111-1111-1111-1111-111111111111";

    private readonly InMemorySongCatalog _catalog = new();
    private readonly InMemoryPuzzleRepository _puzzles = new();
    private readonly InMemorySessionRepository _sessions = new();
    private readonly InMemoryReportRepository _reports = new();
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 11, 13, 10, 0, 0, TimeSpan.Zero));
    private readonly Guid _sessionId;

    public AdminReportsTests()
    {
        for (var i = 1; i <= 8; i++)
            _catalog.Songs.Add(Song.Create(i, $"Song {i}", $"Artist {i}", null, null, 200, 100 - i, SongOrigin.International, _clock.GetUtcNow()));

        var rounds = Enumerable.Range(0, 5).Select(i => PuzzleRound.Create(
            i, _catalog.Songs[i].Id, 50, 5, 6,
            [new RoundMatch(_catalog.Songs[6].Id, 80, 12.5, 18.5), new RoundMatch(_catalog.Songs[7].Id, 60, 3, 9)]));
        var puzzle = Puzzle.CreateDaily(new DateOnly(2026, 11, 13), rounds, _clock.GetUtcNow());
        _puzzles.Puzzles.Add(puzzle);

        var session = GameSession.Start(Player, puzzle, _clock.GetUtcNow());
        session.RegisterPhase1Attempt(0, _catalog.Songs[0].Id, correct: true);
        session.RegisterPhase2(0, _catalog.Songs[5].Id, 42, _clock.GetUtcNow(), matchStartSeconds: 7.5);
        _sessions.Sessions.Add(session);
        _sessionId = session.Id;
    }

    private ReportRoundHandler Reporter() =>
        new(new SessionLoader(_sessions, _puzzles), _reports, _clock);

    private static CancellationToken Ct => CancellationToken.None;

    [Fact]
    public async Task A_player_report_shows_up_for_admins_with_everything_needed_to_judge_it()
    {
        await Reporter().HandleAsync(new ReportRoundCommand(_sessionId, Player, 0, "It sounds the same to me"), Ct);

        var items = await new ListReportsHandler(_reports, _sessions, _puzzles, _catalog)
            .HandleAsync(new ListReportsQuery(ReportStatus.Open), Ct);

        var item = items.ShouldHaveSingleItem();
        item.PlayerNote.ShouldBe("It sounds the same to me");
        item.Score.ShouldBe(42);
        item.Target.Title.ShouldBe("Song 1");
        item.Guess.Title.ShouldBe("Song 6");
        item.GuessMatchStartSeconds.ShouldBe(7.5);
        item.SegmentStartSeconds.ShouldBe(5);
        item.Day.ShouldBe("2026-11-13");
        item.BestMatches.Select(match => (match.Song.Title, match.Score, match.StartSeconds))
            .ShouldBe([("Song 7", 100, (double?)12.5), ("Song 8", 75, 3)]);
    }

    [Fact]
    public async Task Reviewing_moves_a_report_out_of_the_open_list_and_keeps_the_admin_note()
    {
        await Reporter().HandleAsync(new ReportRoundCommand(_sessionId, Player, 0, null), Ct);
        var list = new ListReportsHandler(_reports, _sessions, _puzzles, _catalog);
        var id = (await list.HandleAsync(new ListReportsQuery(ReportStatus.Open), Ct)).Single().Id;

        await new ReviewReportHandler(_reports, _clock).HandleAsync(new ReviewReportCommand(id, "  Checked: fair score  "), Ct);

        (await list.HandleAsync(new ListReportsQuery(ReportStatus.Open), Ct)).ShouldBeEmpty();
        var reviewed = (await list.HandleAsync(new ListReportsQuery(ReportStatus.Reviewed), Ct)).ShouldHaveSingleItem();
        reviewed.AdminNote.ShouldBe("Checked: fair score");
    }

    [Fact]
    public async Task Reviewing_an_unknown_report_is_not_found()
    {
        await Should.ThrowAsync<KeyNotFoundException>(
            () => new ReviewReportHandler(_reports, _clock).HandleAsync(new ReviewReportCommand(Guid.NewGuid(), null), Ct));
    }

    [Fact]
    public async Task A_skipped_phase_two_cannot_be_reported()
    {
        _sessions.Sessions[0].RegisterPhase1Attempt(1, _catalog.Songs[1].Id, correct: true);
        _sessions.Sessions[0].SkipPhase2(1, _clock.GetUtcNow());

        await Should.ThrowAsync<GameRuleException>(
            () => Reporter().HandleAsync(new ReportRoundCommand(_sessionId, Player, 1, null), Ct));
    }

    [Fact]
    public async Task Preview_urls_are_looked_up_by_song_id()
    {
        var handler = new GetSongPreviewHandler(_catalog, new FakeMusicSource());

        var preview = await handler.HandleAsync(new GetSongPreviewQuery(_catalog.Songs[2].Id), Ct);

        preview.Url.ShouldBe("https://cdn/3.mp3");
        await Should.ThrowAsync<KeyNotFoundException>(() => handler.HandleAsync(new GetSongPreviewQuery(Guid.NewGuid()), Ct));
    }
}

internal sealed class InMemoryReportRepository : IReportRepository
{
    public List<RoundReport> Reports { get; } = [];

    public Task<bool> TryAddAsync(RoundReport report, CancellationToken cancellationToken)
    {
        if (Reports.Any(r => r.SessionId == report.SessionId && r.RoundIndex == report.RoundIndex)) return Task.FromResult(false);
        Reports.Add(report);
        return Task.FromResult(true);
    }

    public Task<IReadOnlyList<RoundReport>> ListAsync(ReportStatus status, int take, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<RoundReport>>(
            Reports.Where(r => r.Status == status).OrderByDescending(r => r.CreatedAt).Take(take).ToList());

    public Task<RoundReport?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Reports.FirstOrDefault(r => r.Id == id));

    public Task SaveAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
