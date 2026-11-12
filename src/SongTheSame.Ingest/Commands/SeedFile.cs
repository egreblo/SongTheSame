namespace SongTheSame.Ingest.Commands;

public sealed record SeedFile(int DefaultLimit, List<SeedPlaylist> Playlists);

public sealed record SeedPlaylist(long Id, string Label, string Origin, int? Limit);
