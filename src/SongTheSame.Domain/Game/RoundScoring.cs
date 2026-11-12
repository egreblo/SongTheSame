namespace SongTheSame.Domain.Game;

public static class RoundScoring
{
    private const int PointsLostPerExtraAttempt = 8;

    /// <summary>50 points for a first-attempt guess, 8 fewer for each extra attempt, 0 if never guessed.</summary>
    public static int Phase1Points(int attemptsUsed, bool solved)
    {
        if (!solved) return 0;
        var points = GameRules.MaxPhase1Points - PointsLostPerExtraAttempt * (attemptsUsed - 1);
        return Math.Clamp(points, 1, GameRules.MaxPhase1Points);
    }

    /// <summary>
    /// Rescales a raw similarity score so the best match available in the catalog for this round is worth 100.
    /// This keeps rounds fair: a hard segment whose best twin is only a so-so match is not punished, and the
    /// scale does not drift as the catalog grows.
    /// </summary>
    public static int RelativeToBest(int rawScore, int bestRawScore)
    {
        if (bestRawScore <= 0) return Math.Clamp(rawScore, 1, 100);
        if (rawScore >= bestRawScore) return 100;
        return Math.Clamp((int)Math.Round(100.0 * rawScore / bestRawScore), 1, 100);
    }

    /// <summary>Converts the 1-100 similarity score into 1-50 points.</summary>
    public static int Phase2Points(int similarityScore) =>
        Math.Clamp((int)Math.Round(similarityScore / 2.0, MidpointRounding.AwayFromZero), 0, GameRules.MaxPhase2Points);
}
