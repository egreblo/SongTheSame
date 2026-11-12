namespace SongTheSame.Domain.Puzzles;

public enum PuzzleKind
{
    /// <summary>The puzzle everyone plays on a given game day.</summary>
    Daily = 0,

    /// <summary>A game a player assembled in the editor and shares by code.</summary>
    Custom = 1,
}

/// <summary>Five rounds of songs. Either the daily puzzle or a custom game.</summary>
public sealed class Puzzle
{
    public const int MaxTitleLength = 60;

    private readonly List<PuzzleRound> _rounds = [];

    private Puzzle()
    {
    }

    public Guid Id { get; private set; }

    public PuzzleKind Kind { get; private set; }

    /// <summary>The game day for daily puzzles; the creation date for custom games.</summary>
    public DateOnly Day { get; private set; }

    /// <summary>Short code used in share links of custom games; null for daily puzzles.</summary>
    public string? Code { get; private set; }

    public string? Title { get; private set; }

    public Guid? OwnerId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyList<PuzzleRound> Rounds => _rounds;

    public static Puzzle CreateDaily(DateOnly day, IEnumerable<PuzzleRound> rounds, DateTimeOffset now) =>
        Build(PuzzleKind.Daily, day, null, null, null, rounds, now);

    public static Puzzle CreateCustom(
        string code, string title, Guid ownerId, DateOnly day, IEnumerable<PuzzleRound> rounds, DateTimeOffset now)
    {
        var trimmed = title.Trim();
        if (trimmed.Length == 0) throw new ArgumentException("Title is required.", nameof(title));
        var clipped = trimmed.Length > MaxTitleLength ? trimmed[..MaxTitleLength] : trimmed;
        return Build(PuzzleKind.Custom, day, code, clipped, ownerId, rounds, now);
    }

    private static Puzzle Build(
        PuzzleKind kind, DateOnly day, string? code, string? title, Guid? ownerId, IEnumerable<PuzzleRound> rounds, DateTimeOffset now)
    {
        var puzzle = new Puzzle { Id = Guid.NewGuid(), Kind = kind, Day = day, Code = code, Title = title, OwnerId = ownerId, CreatedAt = now };
        puzzle._rounds.AddRange(rounds.OrderBy(round => round.Index));
        return puzzle;
    }
}
