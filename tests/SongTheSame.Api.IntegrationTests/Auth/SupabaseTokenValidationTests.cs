using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SongTheSame.Api.Auth;

namespace SongTheSame.Api.IntegrationTests.Auth;

public class SupabaseTokenValidationTests : IDisposable
{
    private const string Url = "https://project.supabase.co";
    private const string UserId = "6f1e2d3c-0000-4000-8000-000000000001";

    private readonly RSA _rsa = RSA.Create(2048);
    private readonly RsaSecurityKey _signingKey;
    private readonly SupabaseOptions _options = new() { Url = Url };

    public SupabaseTokenValidationTests() => _signingKey = new RsaSecurityKey(_rsa) { KeyId = "key-1" };

    public void Dispose()
    {
        _rsa.Dispose();
        GC.SuppressFinalize(this);
    }

    private SupabaseJwksProvider Provider(int[]? fetchCounter = null)
    {
        var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(new RsaSecurityKey(_rsa.ExportParameters(false)) { KeyId = "key-1" });
        var json = JsonSerializer.Serialize(new { keys = new[] { jwk } });
        return new SupabaseJwksProvider(_ =>
        {
            if (fetchCounter is not null) fetchCounter[0]++;
            return Task.FromResult(json);
        }, new FakeTimeProvider());
    }

    private string Token(string issuer = Url + "/auth/v1", string audience = "authenticated", TimeSpan? lifetime = null, SecurityKey? key = null)
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Audience = audience,
            Subject = new ClaimsIdentity([new Claim("sub", UserId), new Claim("email", "ana@example.com")]),
            NotBefore = DateTime.UtcNow.AddMinutes(-10),
            Expires = DateTime.UtcNow.Add(lifetime ?? TimeSpan.FromHours(1)),
            SigningCredentials = new SigningCredentials(key ?? _signingKey, SecurityAlgorithms.RsaSha256),
        };
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    private Task<TokenValidationResult> ValidateAsync(string token, SupabaseJwksProvider? provider = null) =>
        new JsonWebTokenHandler().ValidateTokenAsync(token, SupabaseTokenValidation.Create(_options, provider ?? Provider()));

    [Fact]
    public async Task A_token_signed_by_the_project_key_is_accepted()
    {
        var result = await ValidateAsync(Token());

        result.IsValid.ShouldBeTrue();
        AuthenticatedUser.GetId(new ClaimsPrincipal(result.ClaimsIdentity)).ShouldBe(Guid.Parse(UserId));
    }

    [Fact]
    public async Task Tokens_from_another_issuer_or_audience_are_rejected()
    {
        (await ValidateAsync(Token(issuer: "https://evil.example/auth/v1"))).IsValid.ShouldBeFalse();
        (await ValidateAsync(Token(audience: "anon"))).IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task Expired_tokens_are_rejected()
    {
        (await ValidateAsync(Token(lifetime: TimeSpan.FromMinutes(-5)))).IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task Tokens_signed_with_a_different_key_are_rejected()
    {
        using var other = RSA.Create(2048);

        var forged = Token(key: new RsaSecurityKey(other) { KeyId = "key-1" });

        (await ValidateAsync(forged)).IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task Signing_keys_are_fetched_once_and_cached()
    {
        var fetches = new int[1];
        var provider = Provider(fetches);

        await ValidateAsync(Token(), provider);
        await ValidateAsync(Token(), provider);

        fetches[0].ShouldBe(1);
    }

    [Fact]
    public void The_suggested_name_prefers_the_provider_full_name_then_the_email()
    {
        var withName = new ClaimsPrincipal(new ClaimsIdentity([new Claim("user_metadata", "{\"full_name\":\"Ana Horvat\"}"), new Claim("email", "ana@example.com")]));
        var emailOnly = new ClaimsPrincipal(new ClaimsIdentity([new Claim("email", "ana@example.com")]));

        AuthenticatedUser.SuggestedName(withName).ShouldBe("Ana Horvat");
        AuthenticatedUser.SuggestedName(emailOnly).ShouldBe("ana");
    }
}
