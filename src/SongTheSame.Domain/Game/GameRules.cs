namespace SongTheSame.Domain.Game;

/// <summary>Constants that define a daily game. Changing them changes scoring for everyone.</summary>
public static class GameRules
{
    public const int RoundsPerDay = 5;

    /// <summary>Length of the phase 2 target segment: 60 chroma frames, about 6 seconds.</summary>
    public const int SegmentFrames = 60;

    /// <summary>Seconds of the preview unlocked by attempt 1, 2, ... in phase 1.</summary>
    public static readonly IReadOnlyList<int> ClipSeconds = [1, 2, 4, 7, 11, 16];

    public static int MaxPhase1Attempts => ClipSeconds.Count;

    public const int MaxPhase1Points = 50;

    public const int MaxPhase2Points = 50;

    public const int MaxRoundPoints = MaxPhase1Points + MaxPhase2Points;

    public const int MaxDailyPoints = MaxRoundPoints * RoundsPerDay;
}
