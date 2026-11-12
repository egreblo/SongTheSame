using SongTheSame.Application.Abstractions;
using SongTheSame.Domain.Puzzles;

namespace SongTheSame.Application.Features.Game.Common;

/// <summary>Returns the puzzle of a day, generating and storing it the first time it is requested.</summary>
public sealed class DailyPuzzleProvider(
    IPuzzleRepository puzzles,
    ISongCatalog catalog,
    PuzzleRoundBuilder roundBuilder,
    TimeProvider clock)
{
    private const int NoRepeatDays = 90;

    public async Task<Puzzle> GetOrCreateAsync(DateOnly day, CancellationToken cancellationToken)
    {
        var existing = await puzzles.GetByDayAsync(day, cancellationToken);
        if (existing is not null) return existing;

        var songs = await catalog.GetAllAsync(cancellationToken);
        var recent = await puzzles.GetRecentSongIdsAsync(day, NoRepeatDays, cancellationToken);
        var picked = PuzzleSongSelector.Select(day, songs, recent);

        var songsById = songs.ToDictionary(song => song.Id);
        var rounds = new List<PuzzleRound>();
        for (var index = 0; index < picked.Count; index++)
            rounds.Add(await roundBuilder.BuildAsync(index, picked[index], songsById, cancellationToken));

        var puzzle = Puzzle.CreateDaily(day, rounds, clock.GetUtcNow());
        if (await puzzles.TryAddAsync(puzzle, cancellationToken)) return puzzle;

        return await puzzles.GetByDayAsync(day, cancellationToken)
            ?? throw new InvalidOperationException($"Puzzle for {day} could not be created.");
    }
}
