using SongTheSame.Application.Abstractions;

namespace SongTheSame.Application.Features.Catalog.GetSongPreview;

public sealed record GetSongPreviewQuery(Guid SongId);

public sealed record SongPreview(string Url);

/// <summary>A currently valid 30-second preview URL for a catalog song (Deezer URLs expire), fetched when a player presses play.</summary>
public sealed class GetSongPreviewHandler(ISongCatalog catalog, IMusicSource music) : IHandler<GetSongPreviewQuery, SongPreview>
{
    public async Task<SongPreview> HandleAsync(GetSongPreviewQuery request, CancellationToken cancellationToken)
    {
        var songs = await catalog.GetByIdsAsync([request.SongId], cancellationToken);
        var song = songs.Count > 0 ? songs[0] : throw new KeyNotFoundException("Song not found.");

        var url = await music.GetPreviewUrlAsync(song.DeezerId, cancellationToken)
            ?? throw new KeyNotFoundException("This song has no preview right now.");
        return new SongPreview(url);
    }
}
