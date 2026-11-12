using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Game.Common;

namespace SongTheSame.Application.Features.Game.RefreshMatches;

public sealed record RefreshMatchesCommand(DateOnly FromDay);

/// <summary>
/// Recomputes the stored best twins of puzzles from a day onward, keeping their songs, segments and players' games.
/// Run it after the catalog grew or the similarity calibration changed, so the scores and the twin list stay consistent.
/// Returns how many rounds were refreshed.
/// </summary>
public sealed class RefreshMatchesHandler(
    IPuzzleRepository puzzles,
    ISongCatalog catalog,
    PuzzleRoundBuilder roundBuilder) : IHandler<RefreshMatchesCommand, int>
{
    public async Task<int> HandleAsync(RefreshMatchesCommand request, CancellationToken cancellationToken)
    {
        var songsById = (await catalog.GetAllAsync(cancellationToken)).ToDictionary(song => song.Id);
        var refreshed = 0;

        foreach (var puzzle in await puzzles.ListTrackedFromDayAsync(request.FromDay, cancellationToken))
        {
            foreach (var round in puzzle.Rounds)
            {
                var matches = await roundBuilder.FindMatchesAsync(songsById[round.SongId], round.SegmentStartFrame, songsById, cancellationToken);
                round.ReplaceBestMatches(matches);
                refreshed++;
            }

            await puzzles.SaveAsync(cancellationToken);
        }

        return refreshed;
    }
}
