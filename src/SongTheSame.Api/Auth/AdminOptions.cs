using System.Security.Claims;

namespace SongTheSame.Api.Auth;

/// <summary>
/// Who may use the admin pages ("Admin" configuration section): sign-in e-mails and/or Supabase user ids.
/// Example environment variables: Admin__Emails__0=ana@example.com, Admin__UserIds__0=6f1e...
/// </summary>
public sealed class AdminOptions
{
    public const string SectionName = "Admin";
    public const string PolicyName = "Admin";

    public string[] Emails { get; set; } = [];

    public string[] UserIds { get; set; } = [];

    public bool IsAdmin(ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true) return false;

        var id = user.FindFirstValue("sub");
        if (id is not null && UserIds.Contains(id, StringComparer.OrdinalIgnoreCase)) return true;

        var email = user.FindFirstValue("email");
        return email is not null && Emails.Contains(email, StringComparer.OrdinalIgnoreCase);
    }
}
