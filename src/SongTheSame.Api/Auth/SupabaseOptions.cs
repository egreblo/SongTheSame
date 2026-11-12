namespace SongTheSame.Api.Auth;

/// <summary>Settings of the Supabase project that issues sign-in tokens ("Supabase" configuration section).</summary>
public sealed class SupabaseOptions
{
    public const string SectionName = "Supabase";

    /// <summary>Project URL, for example https://abcdefgh.supabase.co. Leave empty to disable sign-in.</summary>
    public string Url { get; set; } = "";

    /// <summary>Only for projects that still sign tokens with the legacy shared JWT secret (HS256).</summary>
    public string? JwtSecret { get; set; }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Url);

    public string Issuer => $"{Url.TrimEnd('/')}/auth/v1";

    public string JwksUrl => $"{Issuer}/.well-known/jwks.json";
}
