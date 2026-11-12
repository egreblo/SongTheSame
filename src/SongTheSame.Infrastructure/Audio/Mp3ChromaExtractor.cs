using NLayer;
using SongTheSame.Application.Abstractions;
using SongTheSame.Audio;

namespace SongTheSame.Infrastructure.Audio;

/// <summary>Decodes mp3 with a managed decoder (no ffmpeg needed in production) and extracts chroma.</summary>
internal sealed class Mp3ChromaExtractor : IChromaExtractor
{
    private const int ReadChunkSamples = 16384;

    private readonly ChromaOptions _options = new();
    private readonly ChromaExtractor _extractor;

    public Mp3ChromaExtractor() => _extractor = new ChromaExtractor(_options);

    public ChromaSequence Extract(byte[] encodedAudio)
    {
        using var stream = new MemoryStream(encodedAudio);
        using var mpeg = new MpegFile(stream);

        var interleaved = ReadAll(mpeg);
        var mono = PcmResampler.DownmixToMono(interleaved, mpeg.Channels);
        var resampled = PcmResampler.Resample(mono, mpeg.SampleRate, _options.SampleRate);
        return _extractor.Extract(resampled);
    }

    private static float[] ReadAll(MpegFile mpeg)
    {
        var samples = new List<float>();
        var chunk = new float[ReadChunkSamples];
        int read;
        while ((read = mpeg.ReadSamples(chunk, 0, chunk.Length)) > 0)
            samples.AddRange(chunk.AsSpan(0, read));
        return [.. samples];
    }
}
