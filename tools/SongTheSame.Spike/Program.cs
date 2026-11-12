using System.Diagnostics;
using SongTheSame.Spike;

// Usage: dotnet run --project tools/SongTheSame.Spike -- [fetch|evaluate|sanity|all]
var command = args.Length > 0 ? args[0] : "all";
var cacheDirectory = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "cache");

using var http = new HttpClient();
http.DefaultRequestHeaders.UserAgent.ParseAdd("SongTheSameSpike/0.1");
var cache = new PreviewCache(Path.GetFullPath(cacheDirectory), new DeezerPreviewClient(http));

if (command is "fetch" or "all")
    await cache.FetchAllAsync(SpikeCatalog.Songs);

if (command is "evaluate" or "sanity" or "all")
{
    var loader = new FeatureLoader(cache);
    var keys = cache.AvailableKeys(SpikeCatalog.Songs);
    var timer = Stopwatch.StartNew();
    var pool = await loader.LoadAsync(keys);
    Console.WriteLine($"\nExtracted chroma for {pool.Count} songs in {timer.Elapsed.TotalSeconds:F1}s");

    if (command is "evaluate" or "all")
    {
        timer.Restart();
        var matrix = SongCostMatrix.Build(pool);
        Console.WriteLine($"Computed {pool.Count * (pool.Count - 1)} pair costs in {timer.Elapsed.TotalSeconds:F1}s");
        PairReport.Print(matrix);
        PairReport.PrintTopMatches(matrix, SpikeCatalog.Pairs.Select(p => p.QueryKey).Distinct());
    }

    if (command is "sanity" or "all")
        await new TransformSanityTest(cache, loader)
            .RunAsync(pool, ["queen_under_pressure", "adele_rolling", "eminem_lose", "abba_dancing"]);
}
