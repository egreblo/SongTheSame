using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Catalog.GetSongPreview;
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

        group.MapGet("/{songId:guid}/preview", async (
                Guid songId, HttpContext http, IHandler<GetSongPreviewQuery, SongPreview> handler, CancellationToken ct) =>
            {
                var preview = await handler.HandleAsync(new GetSongPreviewQuery(songId), ct);
                http.Response.Headers.CacheControl = "private, max-age=300";
                return preview;
            })
            .WithName("GetSongPreview");

        return group;
    }
}
