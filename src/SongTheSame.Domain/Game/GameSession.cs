using SongTheSame.Domain.Puzzles;

namespace SongTheSame.Domain.Game;

/// <summary>One player's attempt at one daily puzzle.</summary>
public sealed class GameSession
{
    private readonly List<RoundProgress> _rounds = [];

    private GameSession()
    {
    }

    public Guid Id { get; private set; }

    /// <summary>"anon:&lt;guid&gt;" for anonymous players, "user:&lt;guid&gt;" for signed-in ones.</summary>
    public string PlayerKey { get; private set; } = "";

    public Guid PuzzleId { get; private set; }

    public DateOnly Day { get; private set; }

    public DateTimeOffset StartedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public IReadOnlyList<RoundProgress> Rounds => _rounds;

    public int TotalPoints => _rounds.Sum(round => round.Points);

    public bool IsCompleted => CompletedAt.HasValue;

    public static GameSession Start(string playerKey, Puzzle puzzle, DateTimeOffset now)
    {
        var session = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerKey = playerKey,
            PuzzleId = puzzle.Id,
            Day = puzzle.Day,
            StartedAt = now,
        };
        session._rounds.AddRange(puzzle.Rounds.Select(round => RoundProgress.Start(round.Index)));
        return session;
    }

    /// <summary>Hands an anonymous game over to the player who just signed in.</summary>
    public void ReassignTo(string playerKey) => PlayerKey = playerKey;

    /// <summary>Returns the round if the player is allowed to act on it (earlier rounds must be finished).</summary>
    public RoundProgress PlayableRound(int index)
    {
        var round = _rounds.FirstOrDefault(r => r.Index == index) ?? throw new GameRuleException("Unknown round.");
        if (_rounds.Any(earlier => earlier.Index < index && !earlier.Phase2Done))
            throw new GameRuleException("Finish the previous rounds first.");
        return round;
    }

    public void RegisterPhase1Attempt(int index, Guid guessedSongId, bool correct) =>
        PlayableRound(index).RegisterPhase1Attempt(guessedSongId, correct);

    public void RegisterPhase2(int index, Guid guessedSongId, int score, DateTimeOffset now)
    {
        PlayableRound(index).RegisterPhase2(guessedSongId, score);
        if (_rounds.All(round => round.Phase2Done)) CompletedAt = now;
    }
}
