using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Catalog.SearchSongs;

namespace SongTheSame.Api.Endpoints;

public static class SongEndpoints
{
    public static IEndpointRouteBuilder MapSongEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/songs").WithTags("Songs");

        group.MapGet("/search", async (
                string q,
                int? limit,
                IHandler<SearchSongsQuery, IReadOnlyList<SongSummary>> handler,
                CancellationToken cancellationToken) =>
                await handler.HandleAsync(new SearchSongsQuery(q, limit ?? 8), cancellationToken))
            .WithName("SearchSongs");

        return group;
    }
}
