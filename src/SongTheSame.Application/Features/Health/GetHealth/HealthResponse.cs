namespace SongTheSame.Application.Features.Health.GetHealth;

public sealed record HealthResponse(string Status, string GameDay, DateTimeOffset ServerTime);
