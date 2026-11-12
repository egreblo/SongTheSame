using SongTheSame.Audio;

namespace SongTheSame.Spike;

public sealed class FeatureLoader(PreviewCache cache)
{
    private readonly ChromaExtractor _extractor = new();
    private readonly ChromaOptions _options = new();

    public async Task<Dictionary<string, ChromaSequence>> LoadAsync(IEnumerable<string> keys)
    {
        var result = new Dictionary<string, ChromaSequence>();
        foreach (var key in keys)
            result[key] = await ExtractAsync(cache.Mp3Path(key));
        return result;
    }

    public async Task<ChromaSequence> ExtractAsync(string path, string? audioFilter = null)
    {
        var samples = await FfmpegDecoder.DecodeAsync(path, _options.SampleRate, audioFilter);
        return _extractor.Extract(samples);
    }
}
