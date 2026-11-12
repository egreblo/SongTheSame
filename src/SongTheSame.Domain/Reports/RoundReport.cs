namespace SongTheSame.Domain.Reports;

public enum ReportStatus
{
    Open = 0,
    Reviewed = 1,
}

/// <summary>A player's claim that a phase 2 score did not reflect how similar their pick really sounded.</summary>
public sealed class RoundReport
{
    private RoundReport()
    {
    }

    public Guid Id { get; private set; }

    public Guid SessionId { get; private set; }

    public int RoundIndex { get; private set; }

    public Guid TargetSongId { get; private set; }

    public Guid GuessedSongId { get; private set; }

    public int Score { get; private set; }

    public string? Note { get; private set; }

    public ReportStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static RoundReport Create(
        Guid sessionId, int roundIndex, Guid targetSongId, Guid guessedSongId, int score, string? note, DateTimeOffset now) =>
        new()
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            RoundIndex = roundIndex,
            TargetSongId = targetSongId,
            GuessedSongId = guessedSongId,
            Score = score,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim()[..Math.Min(note.Trim().Length, 500)],
            Status = ReportStatus.Open,
            CreatedAt = now,
        };
}
