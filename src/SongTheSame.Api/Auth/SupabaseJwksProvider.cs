using Microsoft.IdentityModel.Tokens;

namespace SongTheSame.Api.Auth;

/// <summary>Fetches and caches the public keys Supabase signs its tokens with.</summary>
public sealed class SupabaseJwksProvider(Func<CancellationToken, Task<string>> fetchJwks, TimeProvider clock)
{
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromHours(1);
    private static readonly TimeSpan MinimumRefreshInterval = TimeSpan.FromMinutes(1);

    private readonly object _lock = new();
    private JsonWebKeySet? _keys;
    private DateTimeOffset _fetchedAt = DateTimeOffset.MinValue;

    /// <summary>
    /// Keys matching the token's key id. Refetches when the cache is old, or (at most once a minute)
    /// when an unknown key id shows up, which happens after Supabase rotates its keys.
    /// </summary>
    public IReadOnlyList<SecurityKey> GetKeys(string? keyId)
    {
        lock (_lock)
        {
            var age = clock.GetUtcNow() - _fetchedAt;
            var stale = _keys is null || age > CacheLifetime;
            var unknownKey = keyId is not null && _keys?.Keys.All(key => key.Kid != keyId) == true && age > MinimumRefreshInterval;

            if (stale || unknownKey)
            {
                _keys = new JsonWebKeySet(fetchJwks(CancellationToken.None).GetAwaiter().GetResult());
                _fetchedAt = clock.GetUtcNow();
            }

            return _keys!.GetSigningKeys().Where(key => keyId is null || key.KeyId == keyId).ToList();
        }
    }
}
