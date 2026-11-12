using System.Text.Json.Serialization;
using SongTheSame.Api.Auth;
using SongTheSame.Api.Endpoints;
using SongTheSame.Api.Setup;
using SongTheSame.Application;
using SongTheSame.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
});

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddOpenApi()
    .AddErrorHandling()
    .AddSupabaseAuth(builder.Configuration)
    .AddApiRateLimiting()
    .AddWebClientCors(builder.Configuration);

var app = builder.Build();

app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseRateLimiter();
app.UseCors(CorsSetup.PolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.MapOpenApi();
app.MapHealthEndpoints();
app.MapSongEndpoints();
app.MapDailyEndpoints();
app.MapMeEndpoints();
app.MapAdminEndpoints();
app.MapLeagueEndpoints();
app.MapCustomGameEndpoints();

app.Run();

// Exposed so integration tests can use WebApplicationFactory<Program>.
public partial class Program;
