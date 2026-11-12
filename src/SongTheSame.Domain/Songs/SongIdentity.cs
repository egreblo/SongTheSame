using System.Text.RegularExpressions;

namespace SongTheSame.Domain.Songs;

/// <summary>
/// Decides whether two catalog entries are the same musical work (the original and its "Remastered 2011"
/// edition, a radio edit, a live version, ...), so a guess is not marked wrong over a Deezer duplicate.
/// </summary>
public static partial class SongIdentity
{
    public static bool IsSameWork(Song a, Song b) => a.Id == b.Id || WorkKey(a) == WorkKey(b);

    public static string WorkKey(Song song) => $"{CleanTitle(song.Title)}|{SearchText.Normalize(song.Artist)}";

    private static string CleanTitle(string title)
    {
        var withoutBrackets = BracketedPart().Replace(title, " ");
        var beforeDash = withoutBrackets.Split(" - ", 2, StringSplitOptions.None)[0];
        return SearchText.Normalize(beforeDash);
    }

    [GeneratedRegex(@"\([^)]*\)|\[[^\]]*\]")]
    private static partial Regex BracketedPart();
}
