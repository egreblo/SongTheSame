namespace SongTheSame.Application.Features.Similarity;

/// <param name="MatchStartSeconds">Where in the guessed song preview the most similar part begins.</param>
public sealed record SimilarityScore(int Score, float Cost, double MatchStartSeconds, double MatchEndSeconds);

public sealed record SimilarMatch(Guid SongId, int Score, double StartSeconds, double EndSeconds);

/// <summary>Scores how well a guessed song contains a part similar to a segment of a target song.</summary>
public interface ISimilarityService
{
    Task<SimilarityScore> ScoreAsync(Guid targetSongId, int segmentStartFrame, Guid guessSongId, CancellationToken cancellationToken);

    /// <summary>The catalog songs that match the segment best (the target itself excluded), best first.</summary>
    Task<IReadOnlyList<SimilarMatch>> FindBestMatchesAsync(Guid targetSongId, int segmentStartFrame, int count, CancellationToken cancellationToken);
}
