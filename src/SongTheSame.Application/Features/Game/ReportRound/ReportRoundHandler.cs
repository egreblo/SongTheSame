using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Game.Common;
using SongTheSame.Domain.Game;
using SongTheSame.Domain.Reports;

namespace SongTheSame.Application.Features.Game.ReportRound;

public sealed record ReportRoundCommand(Guid SessionId, string PlayerKey, int RoundIndex, string? Note);

/// <summary>Lets a player flag a round whose phase 2 score looks wrong. Returns true when the report was stored.</summary>
public sealed class ReportRoundHandler(
    SessionLoader loader,
    IReportRepository reports,
    TimeProvider clock) : IHandler<ReportRoundCommand, bool>
{
    public async Task<bool> HandleAsync(ReportRoundCommand request, CancellationToken cancellationToken)
    {
        var (session, puzzle) = await loader.LoadAsync(request.SessionId, request.PlayerKey, cancellationToken);
        var progress = session.Rounds.FirstOrDefault(round => round.Index == request.RoundIndex)
            ?? throw new KeyNotFoundException("Round not found.");
        if (!progress.Phase2Done || progress.Phase2Skipped) throw new GameRuleException("Only rounds with a score can be reported.");

        var round = puzzle.Rounds.Single(r => r.Index == request.RoundIndex);
        var report = RoundReport.Create(
            session.Id, request.RoundIndex, round.SongId, progress.Phase2SongId!.Value, progress.Phase2Score!.Value,
            request.Note, clock.GetUtcNow());
        return await reports.TryAddAsync(report, cancellationToken);
    }
}
