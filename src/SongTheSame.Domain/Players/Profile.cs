namespace SongTheSame.Domain.Players;

/// <summary>A signed-in player. The id is the user id issued by the identity provider (Supabase).</summary>
public sealed class Profile
{
    public const int MaxDisplayNameLength = 40;

    private Profile()
    {
    }

    public Guid UserId { get; private set; }

    public string DisplayName { get; private set; } = "";

    public DateTimeOffset CreatedAt { get; private set; }

    public static Profile Create(Guid userId, string displayName, DateTimeOffset now) =>
        new() { UserId = userId, DisplayName = Normalize(displayName), CreatedAt = now };

    public void Rename(string displayName) => DisplayName = Normalize(displayName);

    private static string Normalize(string displayName)
    {
        var trimmed = displayName.Trim();
        if (trimmed.Length == 0) throw new ArgumentException("Display name is required.", nameof(displayName));
        return trimmed.Length > MaxDisplayNameLength ? trimmed[..MaxDisplayNameLength] : trimmed;
    }
}
