using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SongTheSame.Api.Auth;

public static class SupabaseTokenValidation
{
    public const string Audience = "authenticated";

    public static TokenValidationParameters Create(SupabaseOptions options, SupabaseJwksProvider jwks)
    {
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = options.Issuer,
            ValidateAudience = true,
            ValidAudience = Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            IssuerSigningKeyResolver = (_, _, keyId, _) => TryGetKeys(jwks, keyId),
        };

        if (!string.IsNullOrWhiteSpace(options.JwtSecret))
            parameters.IssuerSigningKeys = [new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.JwtSecret))];

        return parameters;
    }

    /// <summary>A JWKS outage must not crash validation; returning null lets the legacy shared secret (if any) still work.</summary>
    private static IEnumerable<SecurityKey>? TryGetKeys(SupabaseJwksProvider jwks, string? keyId)
    {
        try
        {
            return jwks.GetKeys(keyId);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
}
