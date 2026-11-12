using SongTheSame.Application.Abstractions;

namespace SongTheSame.Application.Features.CustomGames.ListMyCustomGames;

public sealed record ListMyCustomGamesQuery(Guid OwnerId);

public sealed class ListMyCustomGamesHandler(IPuzzleRepository puzzles)
    : IHandler<ListMyCustomGamesQuery, IReadOnlyList<CustomGameSummary>>
{
    public async Task<IReadOnlyList<CustomGameSummary>> HandleAsync(ListMyCustomGamesQuery request, CancellationToken cancellationToken)
    {
        var games = await puzzles.ListCustomByOwnerAsync(request.OwnerId, cancellationToken);
        return games.Select(game => new CustomGameSummary(game.Code!, game.Title!, game.CreatedAt)).ToList();
    }
}
