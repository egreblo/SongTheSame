using System.Security.Claims;
using System.Text.Json;

namespace SongTheSame.Api.Auth;

/// <summary>Reads the signed-in user from the validated token.</summary>
public static class AuthenticatedUser
{
    public static Guid? GetId(ClaimsPrincipal user) =>
        Guid.TryParse(user.FindFirstValue("sub"), out var id) ? id : null;

    /// <summary>A friendly name from the token: the provider's full name, else the part of the e-mail before "@".</summary>
    public static string SuggestedName(ClaimsPrincipal user)
    {
        var metadata = user.FindFirstValue("user_metadata");
        if (!string.IsNullOrWhiteSpace(metadata))
        {
            try
            {
                using var document = JsonDocument.Parse(metadata);
                foreach (var key in new[] { "full_name", "name" })
                    if (document.RootElement.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String
                        && !string.IsNullOrWhiteSpace(value.GetString()))
                        return value.GetString()!;
            }
            catch (JsonException)
            {
                // Malformed metadata: fall back to the e-mail below.
            }
        }

        var email = user.FindFirstValue("email");
        return email is null ? "" : email.Split('@')[0];
    }
}
