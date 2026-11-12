namespace SongTheSame.Audio;

public static class PcmResampler
{
    /// <summary>Averages interleaved channels into a single mono channel.</summary>
    public static float[] DownmixToMono(ReadOnlySpan<float> interleaved, int channels)
    {
        if (channels == 1) return interleaved.ToArray();

        var mono = new float[interleaved.Length / channels];
        for (var i = 0; i < mono.Length; i++)
        {
            var sum = 0f;
            for (var c = 0; c < channels; c++) sum += interleaved[i * channels + c];
            mono[i] = sum / channels;
        }

        return mono;
    }

    /// <summary>
    /// Resamples mono audio by linear interpolation. When reducing the rate, each output sample
    /// averages the input span it covers, which acts as a cheap anti-aliasing low-pass filter.
    /// </summary>
    public static float[] Resample(ReadOnlySpan<float> mono, int fromRate, int toRate)
    {
        if (fromRate == toRate) return mono.ToArray();

        var ratio = (double)fromRate / toRate;
        var outputLength = (int)(mono.Length / ratio);
        var output = new float[outputLength];

        for (var i = 0; i < outputLength; i++)
        {
            var start = i * ratio;
            output[i] = ratio > 1 ? AverageSpan(mono, start, start + ratio) : Interpolate(mono, start);
        }

        return output;
    }

    private static float AverageSpan(ReadOnlySpan<float> samples, double from, double to)
    {
        var first = (int)from;
        var last = Math.Min(samples.Length - 1, (int)Math.Ceiling(to) - 1);
        var sum = 0f;
        for (var i = first; i <= last; i++) sum += samples[i];
        return sum / (last - first + 1);
    }

    private static float Interpolate(ReadOnlySpan<float> samples, double position)
    {
        var index = (int)position;
        var next = Math.Min(index + 1, samples.Length - 1);
        var fraction = (float)(position - index);
        return samples[index] * (1 - fraction) + samples[next] * fraction;
    }
}
