using SongTheSame.Application.Abstractions;
using SongTheSame.Domain.Common;

namespace SongTheSame.Application.Features.Health.GetHealth;

public sealed class GetHealthHandler(TimeProvider clock) : IHandler<GetHealthQuery, HealthResponse>
{
    public Task<HealthResponse> HandleAsync(GetHealthQuery request, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var response = new HealthResponse("Healthy", GameDay.From(now).ToString(), now);
        return Task.FromResult(response);
    }
}
