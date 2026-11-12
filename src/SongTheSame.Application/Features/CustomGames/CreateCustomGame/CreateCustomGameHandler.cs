using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Game.Common;
using SongTheSame.Domain.Common;
using SongTheSame.Domain.Game;
using SongTheSame.Domain.Puzzles;
using SongTheSame.Domain.Songs;

namespace SongTheSame.Application.Features.CustomGames.CreateCustomGame;

public sealed record CreateCustomGameCommand(Guid OwnerId, string Title, IReadOnlyList<Guid> SongIds);

/// <summary>Builds a shareable game from five songs the player picked in the editor.</summary>
public sealed class CreateCustomGameHandler(
    IPuzzleRepository puzzles,
    ISongCatalog catalog,
    PuzzleRoundBuilder roundBuilder,
    TimeProvider clock) : IHandler<CreateCustomGameCommand, CustomGameSummary>
{
    private const int MaxGamesPerPlayer = 50;
    private const int CodeAttempts = 5;

    public async Task<CustomGameSummary> HandleAsync(CreateCustomGameCommand request, CancellationToken cancellationToken)
    {
        if (request.SongIds.Count != GameRules.RoundsPerDay)
            throw new ArgumentException($"A game needs exactly {GameRules.RoundsPerDay} songs.", nameof(request));
        if (string.IsNullOrWhiteSpace(request.Title)) throw new ArgumentException("Title is required.", nameof(request));

        var existing = await puzzles.ListCustomByOwnerAsync(request.OwnerId, cancellationToken);
        if (existing.Count >= MaxGamesPerPlayer) throw new GameRuleException("You have reached the limit of custom games.");

        var songs = await LoadSongsInOrderAsync(request.SongIds, cancellationToken);
        EnsureNoRepeatedSong(songs);

        // Best twins come from the whole catalog, so the lookup needs every song, not just the five chosen.
        var songsById = (await catalog.GetAllAsync(cancellationToken)).ToDictionary(song => song.Id);
        var rounds = new List<PuzzleRound>();
        for (var index = 0; index < songs.Count; index++)
            rounds.Add(await roundBuilder.BuildAsync(index, songs[index], songsById, cancellationToken));

        var now = clock.GetUtcNow();
        var day = GameDay.From(now).Date;
        for (var attempt = 0; attempt < CodeAttempts; attempt++)
        {
            var puzzle = Puzzle.CreateCustom(ShareCode.Generate(), request.Title, request.OwnerId, day, rounds, now);
            if (await puzzles.TryAddAsync(puzzle, cancellationToken))
                return new CustomGameSummary(puzzle.Code!, puzzle.Title!, puzzle.CreatedAt);
        }

        throw new InvalidOperationException("Could not allocate a unique share code.");
    }

    private async Task<List<Song>> LoadSongsInOrderAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken)
    {
        var found = (await catalog.GetByIdsAsync(ids.Distinct().ToList(), cancellationToken)).ToDictionary(song => song.Id);
        return ids.Select(id => found.TryGetValue(id, out var song) ? song : throw new ArgumentException("Unknown song.", nameof(ids))).ToList();
    }

    private static void EnsureNoRepeatedSong(List<Song> songs)
    {
        for (var i = 0; i < songs.Count; i++)
            for (var j = i + 1; j < songs.Count; j++)
                if (SongIdentity.IsSameWork(songs[i], songs[j]))
                    throw new GameRuleException($"\"{songs[i].Title}\" is in the game twice.");
    }
}
