using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace SongTheSame.Api.Auth;

public static class AuthSetup
{
    /// <summary>
    /// Validates Supabase access tokens sent as "Authorization: Bearer". When Supabase is not configured no token
    /// can be valid, so only anonymous play works and the account endpoints answer 401.
    /// </summary>
    public static IServiceCollection AddSupabaseAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(SupabaseOptions.SectionName).Get<SupabaseOptions>() ?? new SupabaseOptions();
        services.AddSingleton(options);

        var jwks = new SupabaseJwksProvider(
            async cancellationToken =>
            {
                using var http = new HttpClient();
                return await http.GetStringAsync(options.JwksUrl, cancellationToken);
            },
            TimeProvider.System);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwt =>
            {
                jwt.MapInboundClaims = false;
                jwt.TokenValidationParameters = options.IsConfigured
                    ? SupabaseTokenValidation.Create(options, jwks)
                    : new TokenValidationParameters
                    {
                        SignatureValidator = (_, _) => throw new SecurityTokenException("Sign-in is not configured."),
                    };
            });
        services.AddAuthorization();
        return services;
    }
}
