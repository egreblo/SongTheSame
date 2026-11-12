using SongTheSame.Audio;

namespace SongTheSame.Spike;

/// <summary>Best-segment cost for every ordered pair of songs in the pool.</summary>
public sealed class SongCostMatrix
{
    private readonly Dictionary<string, Dictionary<string, float>> _costs = new();

    public IReadOnlyList<string> Keys { get; }

    private SongCostMatrix(IReadOnlyList<string> keys) => Keys = keys;

    public static SongCostMatrix Build(IReadOnlyDictionary<string, ChromaSequence> features)
    {
        var keys = features.Keys.ToList();
        var matrix = new SongCostMatrix(keys);
        foreach (var key in keys) matrix._costs[key] = new Dictionary<string, float>();

        Parallel.ForEach(keys, queryKey =>
        {
            var scorer = new SegmentScorer();
            var row = new Dictionary<string, float>();
            foreach (var referenceKey in keys.Where(k => k != queryKey))
                row[referenceKey] = scorer.BestCost(features[queryKey], features[referenceKey]);
            lock (matrix._costs) matrix._costs[queryKey] = row;
        });

        return matrix;
    }

    public float Cost(string query, string reference) => _costs[query][reference];

    /// <summary>1 = the most similar song to the query.</summary>
    public int RankOf(string query, string reference) =>
        1 + _costs[query].Count(pair => pair.Value < _costs[query][reference]);

    public IEnumerable<(string Key, float Cost)> TopMatches(string query, int count) =>
        _costs[query].OrderBy(p => p.Value).Take(count).Select(p => (p.Key, p.Value));

    /// <summary>Share of all ordered pairs that are less similar than this one (0-100).</summary>
    public double Percentile(float cost)
    {
        var all = _costs.Values.SelectMany(row => row.Values).ToList();
        return 100.0 * all.Count(c => c > cost) / all.Count;
    }
}
