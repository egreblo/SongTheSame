namespace SongTheSame.Audio;

/// <summary>A time series of 12-dimensional, L2-normalised chroma (pitch-class) vectors.</summary>
public sealed class ChromaSequence
{
    public const int PitchClasses = 12;

    private readonly float[] _data;

    public ChromaSequence(float[] data, double framesPerSecond)
    {
        if (data.Length % PitchClasses != 0)
            throw new ArgumentException("Data length must be a multiple of 12.", nameof(data));

        _data = data;
        FramesPerSecond = framesPerSecond;
    }

    public double FramesPerSecond { get; }

    public int FrameCount => _data.Length / PitchClasses;

    public double DurationSeconds => FrameCount / FramesPerSecond;

    public ReadOnlySpan<float> Frame(int index) => _data.AsSpan(index * PitchClasses, PitchClasses);

    public float[] ToArray() => (float[])_data.Clone();

    /// <summary>Averages every <paramref name="factor"/> consecutive frames into one (a cheaper, blurrier copy).</summary>
    public ChromaSequence Downsample(int factor)
    {
        var frames = FrameCount / factor;
        var reduced = new float[frames * PitchClasses];
        for (var frame = 0; frame < frames; frame++)
        {
            for (var pc = 0; pc < PitchClasses; pc++)
            {
                var sum = 0f;
                for (var k = 0; k < factor; k++) sum += _data[(frame * factor + k) * PitchClasses + pc];
                reduced[frame * PitchClasses + pc] = sum / factor;
            }
        }

        return new ChromaSequence(reduced, FramesPerSecond / factor);
    }

    public ChromaSequence Slice(int startFrame, int frameCount)
    {
        var slice = new float[frameCount * PitchClasses];
        Array.Copy(_data, startFrame * PitchClasses, slice, 0, slice.Length);
        return new ChromaSequence(slice, FramesPerSecond);
    }
}
