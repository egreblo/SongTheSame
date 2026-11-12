using SongTheSame.Application.Abstractions;
using SongTheSame.Domain.Songs;

namespace SongTheSame.Application.Features.Catalog.IngestTrack;

public sealed record IngestTrackCommand(SourceTrack Track, SongOrigin Origin);

public enum IngestOutcome
{
    Added,
    AlreadyInCatalog,
    NoPreview,
    TooShort,
}
