using SongTheSame.Application.Abstractions;
using SongTheSame.Application.Features.Health.GetHealth;

namespace SongTheSame.Api.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/health", async (
                IHandler<GetHealthQuery, HealthResponse> handler,
                CancellationToken cancellationToken) =>
                await handler.HandleAsync(new GetHealthQuery(), cancellationToken))
            .WithName("GetHealth")
            .WithTags("Health");

        return app;
    }
}
