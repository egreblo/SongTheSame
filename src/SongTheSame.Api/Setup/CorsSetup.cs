namespace SongTheSame.Api.Setup;

public static class CorsSetup
{
    public const string PolicyName = "WebClient";

    /// <summary>Allows the web app's origins, taken from the "Cors:AllowedOrigins" configuration array.</summary>
    public static IServiceCollection AddWebClientCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        return services.AddCors(options => options.AddPolicy(PolicyName, policy =>
            policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()));
    }
}
