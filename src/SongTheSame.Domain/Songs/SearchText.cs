using System.Globalization;
using System.Text;

namespace SongTheSame.Domain.Songs;

/// <summary>Normalises text for accent- and case-insensitive song search ("Štikla" matches "stikla").</summary>
public static class SearchText
{
    private static readonly Dictionary<char, string> SpecialLetters = new()
    {
        ['đ'] = "d",
        ['ł'] = "l",
        ['ø'] = "o",
        ['ß'] = "ss",
        ['æ'] = "ae",
        ['œ'] = "oe",
    };

    public static string Normalize(string text)
    {
        var decomposed = text.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark) continue;

            if (SpecialLetters.TryGetValue(character, out var replacement)) builder.Append(replacement);
            else if (char.IsLetterOrDigit(character)) builder.Append(character);
            else builder.Append(' ');
        }

        return string.Join(' ', builder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    public static IReadOnlyList<string> Tokenize(string text) =>
        Normalize(text).Split(' ', StringSplitOptions.RemoveEmptyEntries);
}
