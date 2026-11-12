using SongTheSame.Domain.Game;
using SongTheSame.Domain.Songs;

namespace SongTheSame.Domain.Puzzles;

/// <summary>
/// Picks the five songs of a day deterministically from the day itself: recognisable songs only, a mix of
/// international and regional music, no artist twice, nothing played recently, easiest (most popular) first.
/// </summary>
public static class PuzzleSongSelector
{
    private const double PopularShare = 0.6;

    public static IReadOnlyList<Song> Select(DateOnly day, IReadOnlyList<Song> catalog, ISet<Guid> recentlyPlayed)
    {
        var random = new Random(day.DayNumber);
        var regionalSlots = 1 + random.Next(2);

        var picked = new List<Song>();
        picked.AddRange(Draw(SongOrigin.Regional, regionalSlots, catalog, recentlyPlayed, picked, random));
        picked.AddRange(Draw(SongOrigin.International, GameRules.RoundsPerDay - picked.Count, catalog, recentlyPlayed, picked, random));

        if (picked.Count < GameRules.RoundsPerDay)
            throw new InvalidOperationException($"Catalog is too small: only {picked.Count} eligible songs for {day}.");

        return picked.OrderByDescending(song => song.Popularity).ToList();
    }

    private static List<Song> Draw(
        SongOrigin origin, int count, IReadOnlyList<Song> catalog, ISet<Guid> recentlyPlayed, List<Song> alreadyPicked, Random random)
    {
        var ofOrigin = catalog.Where(song => song.Origin == origin).OrderByDescending(song => song.Popularity).ToList();
        var popular = ofOrigin.Take(Math.Max(count, (int)(ofOrigin.Count * PopularShare))).ToList();

        var result = new List<Song>();
        foreach (var song in popular.OrderBy(_ => random.Next()))
        {
            if (result.Count == count) break;
            if (recentlyPlayed.Contains(song.Id)) continue;
            if (alreadyPicked.Concat(result).Any(chosen => chosen.Artist.Equals(song.Artist, StringComparison.OrdinalIgnoreCase))) continue;
            result.Add(song);
        }

        return result;
    }
}
