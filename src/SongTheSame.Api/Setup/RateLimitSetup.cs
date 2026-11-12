using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;

namespace SongTheSame.Api.Setup;

/// <summary>Basic abuse protection: a per-client request budget, plus a stricter one for the expensive game builder.</summary>
public static class RateLimitSetup
{
    public const string ExpensivePolicy = "expensive";

    private const int RequestsPerMinute = 300;
    private const int ExpensivePerMinute = 6;

    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services)
    {
        // Behind a proxy (Cloud Run, Fly, ...) the client address arrives in X-Forwarded-For.
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(ClientKey(context), _ => Window(RequestsPerMinute)));
            options.AddPolicy(ExpensivePolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(ClientKey(context), _ => Window(ExpensivePerMinute)));
        });
    }

    private static string ClientKey(HttpContext context) => context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private static FixedWindowRateLimiterOptions Window(int permits) =>
        new() { PermitLimit = permits, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 };
}
