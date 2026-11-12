using SongTheSame.Domain.Common;

namespace SongTheSame.Domain.Tests.Common;

public class GameDayTests
{
    [Fact]
    public void Winter_day_starts_one_hour_ahead_of_UTC()
    {
        var instant = new DateTimeOffset(2026, 11, 12, 23, 30, 0, TimeSpan.Zero);

        GameDay.From(instant).Date.ShouldBe(new DateOnly(2026, 11, 13));
    }

    [Fact]
    public void Summer_day_starts_two_hours_ahead_of_UTC()
    {
        var instant = new DateTimeOffset(2026, 7, 1, 22, 30, 0, TimeSpan.Zero);

        GameDay.From(instant).Date.ShouldBe(new DateOnly(2026, 7, 2));
    }

    [Fact]
    public void Midday_stays_on_the_same_date()
    {
        var instant = new DateTimeOffset(2026, 3, 10, 12, 0, 0, TimeSpan.Zero);

        GameDay.From(instant).ToString().ShouldBe("2026-03-10");
    }
}
