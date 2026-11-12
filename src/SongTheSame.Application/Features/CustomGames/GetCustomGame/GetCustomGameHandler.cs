using SongTheSame.Application.Abstractions;
using SongTheSame.Domain.Common;

namespace SongTheSame.Application.Features.CustomGames.GetCustomGame;

public sealed record GetCustomGameQuery(string Code);

public sealed class GetCustomGameHandler(IPuzzleRepository puzzles, IProfileRepository profiles)
    : IHandler<GetCustomGameQuery, CustomGameInfo>
{
    public async Task<CustomGameInfo> HandleAsync(GetCustomGameQuery request, CancellationToken cancellationToken)
    {
        var puzzle = await puzzles.GetByCodeAsync(ShareCode.Normalize(request.Code), cancellationToken)
            ?? throw new KeyNotFoundException("Game not found.");

        var author = puzzle.OwnerId is { } ownerId ? await profiles.GetAsync(ownerId, cancellationToken) : null;
        return new CustomGameInfo(puzzle.Code!, puzzle.Title!, author?.DisplayName);
    }
}
