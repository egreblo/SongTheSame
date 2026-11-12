using System.Text.Json.Serialization;

namespace SongTheSame.Infrastructure.Deezer;

internal sealed record DeezerPage(
    [property: JsonPropertyName("data")] List<DeezerTrackDto>? Data,
    [property: JsonPropertyName("next")] string? Next,
    [property: JsonPropertyName("error")] DeezerError? Error);

internal sealed record DeezerError(
    [property: JsonPropertyName("code")] int Code,
    [property: JsonPropertyName("message")] string? Message);

internal sealed record DeezerTrackDto(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("duration")] int Duration,
    [property: JsonPropertyName("rank")] int Rank,
    [property: JsonPropertyName("preview")] string? Preview,
    [property: JsonPropertyName("artist")] DeezerArtistDto? Artist,
    [property: JsonPropertyName("album")] DeezerAlbumDto? Album);

internal sealed record DeezerArtistDto([property: JsonPropertyName("name")] string? Name);

internal sealed record DeezerAlbumDto(
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("cover_medium")] string? CoverMedium);
