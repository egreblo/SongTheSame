using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SongTheSame.Api.IntegrationTests;

public class AccountEndpointTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Account_endpoints_require_a_token()
    {
        var client = factory.CreateClient();

        (await client.GetAsync("/api/me")).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task A_garbage_token_is_rejected_when_sign_in_is_not_configured()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", "not-a-real-token");

        (await client.GetAsync("/api/me")).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Starting_a_game_needs_a_player_key()
    {
        var client = factory.CreateClient();

        (await client.PostAsync("/api/daily/session", null)).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
