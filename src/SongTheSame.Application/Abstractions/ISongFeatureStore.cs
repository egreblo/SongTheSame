using SongTheSame.Audio;

namespace SongTheSame.Application.Abstractions;

public sealed record SongChroma(Guid SongId, ChromaSequence Chroma);

public interface ISongFeatureStore
{
    Task<ChromaSequence?> GetAsync(Guid songId, CancellationToken cancellationToken);

    /// <summary>Every song's chroma. Loads the whole catalog into memory; use for batch jobs only.</summary>
    Task<IReadOnlyList<SongChroma>> GetAllAsync(CancellationToken cancellationToken);
}
