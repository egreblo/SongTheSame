using SongTheSame.Domain.Songs;

namespace SongTheSame.Application.Features.Catalog.IngestPlaylist;

public sealed record IngestPlaylistCommand(long PlaylistId, SongOrigin Origin, int Limit);

public sealed record IngestPlaylistResult(int Added, int Skipped, int Failed);
