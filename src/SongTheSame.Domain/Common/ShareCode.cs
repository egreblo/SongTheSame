using System.Security.Cryptography;

namespace SongTheSame.Domain.Common;

/// <summary>Short, unambiguous, random codes for share links and league invites (no 0/O/1/I/L).</summary>
public static class ShareCode
{
    public const int Length = 8;

    private const string Alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";

    public static string Generate() => RandomNumberGenerator.GetString(Alphabet, Length);

    /// <summary>Uppercases and strips spaces and dashes, so pasted codes like "abcd-efgh" still match.</summary>
    public static string Normalize(string code) =>
        new(code.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());
}
