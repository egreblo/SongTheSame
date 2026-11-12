using System.Globalization;

namespace SongTheSame.Domain.Common;

/// <summary>
/// The calendar day a daily puzzle belongs to. A new game day starts at midnight in
/// Europe/Zagreb, regardless of the server's time zone.
/// </summary>
public readonly record struct GameDay(DateOnly Date)
{
    private const string TimeZoneId = "Europe/Zagreb";

    private static readonly TimeZoneInfo GameTimeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);

    public static GameDay From(DateTimeOffset instant)
    {
        var local = TimeZoneInfo.ConvertTime(instant, GameTimeZone);
        return new GameDay(DateOnly.FromDateTime(local.DateTime));
    }

    public override string ToString() => Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
