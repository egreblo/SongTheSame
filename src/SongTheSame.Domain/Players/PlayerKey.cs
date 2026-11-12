using System.Globalization;

namespace SongTheSame.Domain.Players;

/// <summary>Identifies who plays: an anonymous browser ("anon:&lt;guid&gt;") or a signed-in user ("user:&lt;guid&gt;").</summary>
public static class PlayerKey
{
    public static string Anonymous(Guid id) => string.Create(CultureInfo.InvariantCulture, $"anon:{id:D}");

    public static string ForUser(Guid userId) => string.Create(CultureInfo.InvariantCulture, $"user:{userId:D}");

    public static bool TryGetUserId(string playerKey, out Guid userId)
    {
        userId = Guid.Empty;
        return playerKey.StartsWith("user:", StringComparison.Ordinal) && Guid.TryParse(playerKey.AsSpan(5), out userId);
    }
}
