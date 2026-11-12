namespace SongTheSame.Domain.Game;

/// <summary>A player's progress through one round: phase 1 (name the song), then phase 2 (find its twin).</summary>
public sealed class RoundProgress
{
    private RoundProgress()
    {
    }

    public int Index { get; private set; }

    public int Phase1Attempts { get; private set; }

    public bool Phase1Solved { get; private set; }

    public bool Phase1Done { get; private set; }

    /// <summary>Guessed song ids in order; <see cref="Guid.Empty"/> marks a skipped clip.</summary>
    public List<Guid> Phase1Guesses { get; private set; } = [];

    public Guid? Phase2SongId { get; private set; }

    public int? Phase2Score { get; private set; }

    public bool Phase2Done => Phase2Score.HasValue;

    public int Phase1Points => RoundScoring.Phase1Points(Phase1Attempts, Phase1Solved);

    public int Phase2Points => Phase2Score.HasValue ? RoundScoring.Phase2Points(Phase2Score.Value) : 0;

    public int Points => Phase1Points + Phase2Points;

    /// <summary>Seconds of the preview the player may hear right now in phase 1.</summary>
    public int UnlockedClipSeconds =>
        GameRules.ClipSeconds[Math.Min(Phase1Attempts, GameRules.ClipSeconds.Count - 1)];

    public static RoundProgress Start(int index) => new() { Index = index };

    public void RegisterPhase1Attempt(Guid guessedSongId, bool correct)
    {
        if (Phase1Done) throw new GameRuleException("Phase 1 of this round is already finished.");

        Phase1Guesses.Add(guessedSongId);
        Phase1Attempts++;
        if (correct)
        {
            Phase1Solved = true;
            Phase1Done = true;
        }
        else if (Phase1Attempts >= GameRules.MaxPhase1Attempts)
        {
            Phase1Done = true;
        }
    }

    public void RegisterPhase2(Guid guessedSongId, int score)
    {
        if (!Phase1Done) throw new GameRuleException("Finish phase 1 before phase 2.");
        if (Phase2Done) throw new GameRuleException("Phase 2 of this round is already answered.");

        Phase2SongId = guessedSongId;
        Phase2Score = score;
    }
}
