namespace SongTheSame.Spike;

public static class PairReport
{
    public static void Print(SongCostMatrix matrix)
    {
        Console.WriteLine($"\n=== Labeled pairs (pool of {matrix.Keys.Count} songs) ===");
        Console.WriteLine($"{"query",-22} {"reference",-22} {"relation",-20} {"cost",6} {"rank",5} {"score",6}  reverse rank");

        var ranks = new List<int>();
        foreach (var pair in SpikeCatalog.Pairs)
        {
            if (!matrix.Keys.Contains(pair.QueryKey) || !matrix.Keys.Contains(pair.ReferenceKey)) continue;

            var cost = matrix.Cost(pair.QueryKey, pair.ReferenceKey);
            var rank = matrix.RankOf(pair.QueryKey, pair.ReferenceKey);
            var reverse = matrix.RankOf(pair.ReferenceKey, pair.QueryKey);
            ranks.Add(rank);
            Console.WriteLine(
                $"{pair.QueryKey,-22} {pair.ReferenceKey,-22} {pair.Relation,-20} {cost,6:F3} {rank,5} {matrix.Percentile(cost),6:F0}  {reverse}");
        }

        if (ranks.Count > 0)
            Console.WriteLine($"\nmedian rank {Median(ranks)}, in top 3: {ranks.Count(r => r <= 3)}/{ranks.Count}, in top 5: {ranks.Count(r => r <= 5)}/{ranks.Count}");
    }

    public static void PrintTopMatches(SongCostMatrix matrix, IEnumerable<string> queries)
    {
        Console.WriteLine("\n=== Top 3 matches per query ===");
        foreach (var query in queries.Where(matrix.Keys.Contains))
        {
            var top = string.Join(", ", matrix.TopMatches(query, 3).Select(m => $"{m.Key} ({m.Cost:F3})"));
            Console.WriteLine($"{query,-22} -> {top}");
        }
    }

    private static int Median(List<int> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        return sorted[sorted.Count / 2];
    }
}
