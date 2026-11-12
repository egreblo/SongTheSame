namespace SongTheSame.Audio;

public sealed record ChromaOptions(
    int SampleRate = 22050,
    int FrameSize = 4096,
    int HopSize = 2205,
    double MinFrequency = 110.0,
    double MaxFrequency = 3520.0,
    double CompressionGamma = 100.0,
    int SmoothingFrames = 5,
    bool CenterFrames = true);

/// <summary>Turns mono PCM samples into a chroma sequence (STFT, pitch-class folding, log compression, smoothing).</summary>
public sealed class ChromaExtractor
{
    private readonly ChromaOptions _options;
    private readonly FftPlan _fft;
    private readonly float[] _window;
    private readonly int[] _binPitchClass;

    public ChromaExtractor(ChromaOptions? options = null)
    {
        _options = options ?? new ChromaOptions();
        _fft = new FftPlan(_options.FrameSize);
        _window = BuildHannWindow(_options.FrameSize);
        _binPitchClass = BuildBinMap(_options);
    }

    public ChromaSequence Extract(ReadOnlySpan<float> samples)
    {
        var frameCount = samples.Length < _options.FrameSize
            ? 0
            : 1 + (samples.Length - _options.FrameSize) / _options.HopSize;

        var raw = new float[frameCount * ChromaSequence.PitchClasses];
        var real = new float[_options.FrameSize];
        var imaginary = new float[_options.FrameSize];

        for (var frame = 0; frame < frameCount; frame++)
        {
            ComputeSpectrum(samples, frame * _options.HopSize, real, imaginary);
            FoldIntoChroma(real, imaginary, raw.AsSpan(frame * ChromaSequence.PitchClasses, ChromaSequence.PitchClasses));
        }

        var smoothed = ChromaSmoothing.MovingAverage(raw, _options.SmoothingFrames);
        if (_options.CenterFrames) ChromaSmoothing.CenterFrames(smoothed);
        ChromaSmoothing.NormalizeFrames(smoothed);
        return new ChromaSequence(smoothed, (double)_options.SampleRate / _options.HopSize);
    }

    private void ComputeSpectrum(ReadOnlySpan<float> samples, int offset, float[] real, float[] imaginary)
    {
        for (var i = 0; i < _options.FrameSize; i++)
        {
            real[i] = samples[offset + i] * _window[i];
            imaginary[i] = 0f;
        }

        _fft.Forward(real, imaginary);
    }

    private void FoldIntoChroma(float[] real, float[] imaginary, Span<float> chroma)
    {
        for (var bin = 0; bin < _binPitchClass.Length; bin++)
        {
            var pitchClass = _binPitchClass[bin];
            if (pitchClass < 0) continue;
            var magnitude = MathF.Sqrt(real[bin] * real[bin] + imaginary[bin] * imaginary[bin]);
            chroma[pitchClass] += MathF.Log(1f + (float)_options.CompressionGamma * magnitude / _options.FrameSize);
        }
    }

    private static float[] BuildHannWindow(int size)
    {
        var window = new float[size];
        for (var i = 0; i < size; i++)
            window[i] = (float)(0.5 - 0.5 * Math.Cos(2.0 * Math.PI * i / (size - 1)));
        return window;
    }

    private static int[] BuildBinMap(ChromaOptions options)
    {
        var map = new int[options.FrameSize / 2];
        for (var bin = 0; bin < map.Length; bin++)
        {
            var frequency = (double)bin * options.SampleRate / options.FrameSize;
            if (frequency < options.MinFrequency || frequency > options.MaxFrequency)
            {
                map[bin] = -1;
                continue;
            }

            var midi = (int)Math.Round(69 + 12 * Math.Log2(frequency / 440.0));
            map[bin] = ((midi % 12) + 12) % 12;
        }

        return map;
    }
}
