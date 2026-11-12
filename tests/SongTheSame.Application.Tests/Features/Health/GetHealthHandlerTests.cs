using Microsoft.Extensions.Time.Testing;
using SongTheSame.Application.Features.Health.GetHealth;

namespace SongTheSame.Application.Tests.Features.Health;

public class GetHealthHandlerTests
{
    [Fact]
    public async Task Reports_healthy_and_the_current_game_day()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 11, 12, 23, 30, 0, TimeSpan.Zero));
        var handler = new GetHealthHandler(clock);

        var response = await handler.HandleAsync(new GetHealthQuery(), CancellationToken.None);

        response.Status.ShouldBe("Healthy");
        response.GameDay.ShouldBe("2026-11-13");
    }
}
