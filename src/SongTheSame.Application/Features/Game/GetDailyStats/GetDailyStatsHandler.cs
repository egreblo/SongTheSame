using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Game.Common;
using SongTheSame.Domain.Game;

namespace SongTheSame.Application.Features.Game.GetDailyStats;

public sealed record GetDailyStatsQuery(Guid SessionId, string PlayerKey);

public sealed record HistogramBin(int From, int To, int Count);

/// <param name="BeatPercent">Share of the other finished players this player scored higher than; null when nobody else played.</param>
public sealed record DailyStatsView(
    int YourPoints, int PlayerCount, int? BeatPercent, double AveragePoints, IReadOnlyList<HistogramBin> Histogram);

/// <summary>Compares a finished game with everyone else who finished the same day's puzzle.</summary>
public sealed class GetDailyStatsHandler(SessionLoader loader, ISessionRepository sessions)
    : IHandler<GetDailyStatsQuery, DailyStatsView>
{
    private const int BinCount = 10;

    public async Task<DailyStatsView> HandleAsync(GetDailyStatsQuery request, CancellationToken cancellationToken)
    {
        var (session, _) = await loader.LoadAsync(request.SessionId, request.PlayerKey, cancellationToken);
        if (!session.IsCompleted) throw new GameRuleException("Finish all rounds to see how you compare.");

        var totals = await sessions.GetCompletedTotalsAsync(session.PuzzleId, cancellationToken);
        var others = totals.Count - 1;
        int? beatPercent = others > 0
            ? (int)Math.Round(100.0 * totals.Count(total => total < session.TotalPoints) / others)
            : null;

        return new DailyStatsView(session.TotalPoints, totals.Count, beatPercent, totals.Average(), Bin(totals));
    }

    private static List<HistogramBin> Bin(IReadOnlyList<int> totals)
    {
        var width = GameRules.MaxDailyPoints / BinCount;
        return Enumerable.Range(0, BinCount)
            .Select(bin =>
            {
                var from = bin * width;
                var to = bin == BinCount - 1 ? GameRules.MaxDailyPoints : from + width - 1;
                return new HistogramBin(from, to, totals.Count(total => total >= from && total <= to));
            })
            .ToList();
    }
}
