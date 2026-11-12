using System.Globalization;
using SongTheSame.Audio;

namespace SongTheSame.Spike;

/// <summary>
/// Sanity check for the tempo/key tolerance: a song transposed or sped up must still be
/// recognised as itself among all other songs.
/// </summary>
public sealed class TransformSanityTest(PreviewCache cache, FeatureLoader loader)
{
    private static readonly double SemitoneUp2 = Math.Pow(2, 2.0 / 12);

    private static readonly (string Name, string Filter)[] Transforms =
    [
        ("+2 semitones", PitchFilter(SemitoneUp2, 1.0)),
        ("tempo x1.15", string.Create(CultureInfo.InvariantCulture, $"atempo=1.15")),
        ("+2 semitones & tempo x1.15", PitchFilter(SemitoneUp2, 1.15)),
        ("tempo x0.85", string.Create(CultureInfo.InvariantCulture, $"atempo=0.85")),
    ];

    public async Task RunAsync(IReadOnlyDictionary<string, ChromaSequence> pool, IEnumerable<string> keys)
    {
        Console.WriteLine("\n=== Transform sanity (original segment vs altered full preview; rank 1 is perfect) ===");
        var scorer = new SegmentScorer();

        foreach (var key in keys.Where(pool.ContainsKey))
        {
            foreach (var (name, filter) in Transforms)
            {
                var altered = await loader.ExtractAsync(cache.Mp3Path(key), filter);
                var selfCost = scorer.BestCost(pool[key], altered);
                var beaten = pool.Where(p => p.Key != key).Count(p => scorer.BestCost(pool[key], p.Value) < selfCost);
                Console.WriteLine($"{key,-22} {name,-28} cost {selfCost,6:F3}  rank {beaten + 1}");
            }
        }
    }

    private static string PitchFilter(double pitchRatio, double tempoRatio) =>
        string.Create(CultureInfo.InvariantCulture,
            $"aresample=44100,asetrate={44100 * pitchRatio:F2},aresample=22050,atempo={tempoRatio / pitchRatio:F5}");
}
