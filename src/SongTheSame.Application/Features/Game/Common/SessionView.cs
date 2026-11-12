using SongTheSame.Application.Features.Catalog.SearchSongs;

namespace SongTheSame.Application.Features.Game.Common;

public enum RoundStatus
{
    Locked,
    Phase1,
    Phase2,
    Done,
}

public sealed record SessionView(
    Guid SessionId,
    string Day,
    int TotalPoints,
    int MaxPoints,
    bool Completed,
    IReadOnlyList<RoundView> Rounds);

public sealed record RoundView(
    int Index,
    RoundStatus Status,
    string? AudioUrl,
    int Attempts,
    int MaxAttempts,
    int UnlockedClipSeconds,
    IReadOnlyList<int> ClipSeconds,
    IReadOnlyList<GuessView> Guesses,
    RevealView? Reveal,
    Phase2View? Phase2,
    int Phase1Points,
    int Points);

/// <summary>A phase 1 attempt; <c>Song</c> is null when the player skipped the clip.</summary>
public sealed record GuessView(SongSummary? Song);

public sealed record RevealView(SongSummary Song, bool Solved, double SegmentStartSeconds, double SegmentSeconds);

/// <param name="Guess">Null when the player skipped finding a twin.</param>
/// <param name="GuessMatchStartSeconds">Where the guessed song is most similar to the segment, for playback.</param>
public sealed record Phase2View(
    SongSummary? Guess, bool Skipped, int Score, int Points, double? GuessMatchStartSeconds, IReadOnlyList<MatchView> BestMatches);

/// <param name="StartSeconds">Where in the song preview the most similar part begins, when known.</param>
public sealed record MatchView(SongSummary Song, int Score, double? StartSeconds);
