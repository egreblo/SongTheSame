using SongTheSame.Api.Auth;
using SongTheSame.Domain.Players;

namespace SongTheSame.Api.Setup;

/// <summary>Works out who is playing from the request.</summary>
public static class PlayerIdentity
{
    public const string AnonymousHeader = "X-Player-Key";

    /// <summary>"user:&lt;id&gt;" for a signed-in player, else "anon:&lt;id&gt;" from the browser's header, else null.</summary>
    public static string? FromRequest(HttpContext context)
    {
        var userId = AuthenticatedUser.GetId(context.User);
        return userId is not null ? PlayerKey.ForUser(userId.Value) : AnonymousFromRequest(context);
    }

    /// <summary>The anonymous key from the header, even for signed-in players (used to claim their earlier games).</summary>
    public static string? AnonymousFromRequest(HttpContext context)
    {
        var header = context.Request.Headers[AnonymousHeader].ToString();
        return Guid.TryParse(header, out var id) ? PlayerKey.Anonymous(id) : null;
    }
}
